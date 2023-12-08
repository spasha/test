using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FSC.ServiceBus.Abstractions;
using FSC.ServiceBus.Abstractions.MessageReception;
using FSC.ServiceBus.Dispatch;
using FSC.ServiceBus.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MessageContext = FSC.ServiceBus.Abstractions.MessageContext;
using log4net;


namespace FSC.ServiceBus
{

  public class ReceiverWrapper : IWrapper
  {
    //private readonly ILogger<ReceiverWrapper> _logger;
    private static readonly ILog Log = LogManager.GetLogger(typeof(ReceiverWrapper));

    private readonly ServiceBusClient _client;
    private readonly ServiceBusOptions _parentOptions;
    private readonly IServiceProvider _provider;
    private readonly ComposedReceiverOptions _composedOptions;

    private Func<ProcessErrorEventArgs, Task> _onExceptionReceivedHandler;

    public ReceiverWrapper(ServiceBusClient client,
        ComposedReceiverOptions options,
        ServiceBusOptions parentOptions,
        IServiceProvider provider)
    {
      _client = client;
      _composedOptions = options;
      _parentOptions = parentOptions;
      _provider = provider;
     // _logger = _provider.GetRequiredService<ILogger<ReceiverWrapper>>();
    }

    public string ResourceId => _composedOptions.ResourceId;
    public ClientType ClientType => _composedOptions.ClientType;

    internal ServiceBusProcessor ProcessorClient { get; private set; }
    internal ServiceBusSessionProcessor SessionProcessorClient { get; private set; }

    public async Task Initialize()
    {
      Log.Info($"[FSC.ServiceBus] Initialization of receiver client '{ResourceId}': Start");
      if (_parentOptions.Settings.Enabled == false)
      {
        Log.Info(
            $"[FSC.ServiceBus] Initialization of client '{ResourceId}': Client deactivated through configuration");
        return;
      }

      await RegisterMessageHandler().ConfigureAwait(false);
      Log.Info($"[FSC.ServiceBus] Initialization of client '{ResourceId}': Success");
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
      if (ProcessorClient != null && ProcessorClient.IsClosed == false)
      {
        try
        {
          await ProcessorClient.CloseAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          Log.Error($"[FSC.ServiceBus] Client {ResourceId} couldn't close properly. Error: {ex.Message}");
        }
      }

      if (SessionProcessorClient != null && SessionProcessorClient.IsClosed == false)
      {
        try
        {
          await SessionProcessorClient.CloseAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          Log.Error($"[FSC.ServiceBus] Client {ResourceId} couldn't close properly. Error: {ex.Message}");
        }
      }
    }

    private async Task RegisterMessageHandler()
    {
      if (_parentOptions.Settings.ReceiveMessages == false)
      {
        return;
      }

      if (_composedOptions.SessionMode)
      {
        if (_composedOptions.FirstOption is QueueOptions)
        {
          var queueOptions = _composedOptions.FirstOption as QueueOptions;
          SessionProcessorClient = _client.CreateSessionProcessor(queueOptions.QueueName, _composedOptions.SessionProcessorOptions);

        }
        else if (_composedOptions.FirstOption is SubscriptionOptions)
        {
          var subscriptionOptions = _composedOptions.FirstOption as SubscriptionOptions;
          SessionProcessorClient = _client.CreateSessionProcessor(
              subscriptionOptions.TopicName,
              subscriptionOptions.SubscriptionName,
              _composedOptions.SessionProcessorOptions);
        }
        else
        {
          SessionProcessorClient = SessionProcessorClient;
        }

        SessionProcessorClient.ProcessErrorAsync += OnExceptionOccured;
        SessionProcessorClient.ProcessMessageAsync += args => OnMessageReceived(new MessageContext(args, _composedOptions.ClientType, _composedOptions.ResourceId));
        await SessionProcessorClient.StartProcessingAsync().ConfigureAwait(false);
      }
      else
      {
        if (_composedOptions.FirstOption is QueueOptions)
        {
          var queueOptions = _composedOptions.FirstOption as QueueOptions;
          ProcessorClient = _client.CreateProcessor(queueOptions.QueueName, _composedOptions.ProcessorOptions);

        }
        else if (_composedOptions.FirstOption is SubscriptionOptions)
        {
          var subscriptionOptions = _composedOptions.FirstOption as SubscriptionOptions;
          ProcessorClient = _client.CreateProcessor(
              subscriptionOptions.TopicName,
              subscriptionOptions.SubscriptionName,
              _composedOptions.ProcessorOptions);

        }
        else
        {
          ProcessorClient = ProcessorClient;
        }

        //ProcessorClient.ProcessErrorAsync += OnExceptionOccured;
        //ProcessorClient.ProcessMessageAsync += args => OnMessageReceived(new MessageContext(args, _composedOptions.ClientType, _composedOptions.ResourceId));
        //ProcessorClient = _composedOptions.FirstOption switch
        //{
        //  QueueOptions queueOptions => _client.CreateProcessor(queueOptions.QueueName, _composedOptions.ProcessorOptions),
        //  SubscriptionOptions subscriptionOptions => _client.CreateProcessor(
        //      subscriptionOptions.TopicName,
        //      subscriptionOptions.SubscriptionName,
        //      _composedOptions.ProcessorOptions),
        //  _ => ProcessorClient
        //};
        ProcessorClient.ProcessErrorAsync += OnExceptionOccured;
        ProcessorClient.ProcessMessageAsync += args => OnMessageReceived(new MessageContext(args, _composedOptions.ClientType, _composedOptions.ResourceId));
        await ProcessorClient.StartProcessingAsync().ConfigureAwait(false);
      }

      _onExceptionReceivedHandler = _ => Task.CompletedTask;

      if (_composedOptions.ExceptionHandlerType != null)
      {
        _onExceptionReceivedHandler = CallDefinedExceptionHandler;
      }
    }

    /// <summary>
    ///     Called when a message is received.
    ///     Will create a scope & call the message handler associated with this <see cref="ReceiverWrapper" />.
    /// </summary>
    /// <returns></returns>
    private async Task OnMessageReceived(MessageContext context)
    {
      var sw = new Stopwatch();
      var scopeValues = new Dictionary<string, string>
      {
        ["FSCSB_Client"] = ClientType.ToString(),
        ["FSCSB_ResourceId"] = ResourceId,
        ["FSCSB_Handler"] = _composedOptions.MessageHandlerType.FullName,
        ["FSCSB_MessageId"] = context.Message.MessageId,
        ["FSCSB_SessionId"] = context.SessionArgs?.SessionId ?? "none",
      };
      //using (_logger.BeginScope(scopeValues))
      using (var scope = _provider.CreateScope())
      {
        var metadataAccessor =
            (MessageMetadataAccessor)scope.ServiceProvider.GetRequiredService<IMessageMetadataAccessor>();
        metadataAccessor.SetData(context);
        var listeners = scope.ServiceProvider.GetRequiredService<IEnumerable<IServiceBusEventListener>>();
        var executionStartedArgs = new ExecutionStartedArgs(ClientType, ResourceId, _composedOptions.MessageHandlerType, context.Message);
        foreach (var listener in listeners)
        {
          await listener.OnExecutionStart(executionStartedArgs).ConfigureAwait(false);
        }
        Log.Info("$[FSC.ServiceBus] New message received from {ClientType} '{ResourceId}' : {context.Message.Subject}");

        var messageHandler = (IMessageHandler)scope.ServiceProvider.GetRequiredService(_composedOptions.MessageHandlerType);
        sw.Start();
        try
        {
          await messageHandler.HandleMessageAsync(context).ConfigureAwait(false);
        }
        catch (Exception ex) when (LogError(ex))
        {
          var executionFailedArgs = new ExecutionFailedArgs(ClientType, ResourceId, _composedOptions.MessageHandlerType, context.Message, ex);
          foreach (var listener in listeners)
          {
            await listener.OnExecutionFailed(executionFailedArgs).ConfigureAwait(false);
          }
          throw;
        }
        finally
        {
          sw.Stop();
        }
        var executionSucceededArgs = new ExecutionSucceededArgs(ClientType, ResourceId, _composedOptions.MessageHandlerType, context.Message, sw.ElapsedMilliseconds);
        foreach (var listener in listeners)
        {
          await listener.OnExecutionSuccess(executionSucceededArgs).ConfigureAwait(false);
        }
        Log.Info("[FSC.ServiceBus] Message finished execution in {sw.ElapsedMilliseconds} milliseconds");
      }
    }

        /// <summary>
        ///     workaround to attach the log scope to the logged exception
        ///     https://andrewlock.net/how-to-include-scopes-when-logging-exceptions-in-asp-net-core/
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private bool LogError(Exception ex)
        {
            Log.Error(ex.Message);
            return true;
        }

        /// <summary>
        ///     Called whenever an exception occurs during the handling of a message.
        /// </summary>
        /// <param name="exceptionEvent"></param>
        /// <returns></returns>
        private async Task OnExceptionOccured(ProcessErrorEventArgs exceptionEvent)
    {
      var json = JsonSerializer.Serialize(new
      {
        exceptionEvent.ErrorSource,
        exceptionEvent.FullyQualifiedNamespace,
        exceptionEvent.EntityPath
      }, new JsonSerializerOptions()
      {
        WriteIndented = true
      });
      Log.Error($"[FSC.ServiceBus] Exception occured during message treatment of {_composedOptions.ClientType} '{ResourceId}'.\n"
          + $"Message : {exceptionEvent.Exception.Message}\n"
          + $"Context:\n{json}");

      await _onExceptionReceivedHandler(exceptionEvent).ConfigureAwait(false);
    }

    private async Task CallDefinedExceptionHandler(ProcessErrorEventArgs exceptionEvent)
    {
      var userDefinedExceptionHandler =
          (IExceptionHandler)_provider.GetService(_composedOptions.ExceptionHandlerType);
      await userDefinedExceptionHandler.HandleExceptionAsync(exceptionEvent).ConfigureAwait(false);
    }
  }
}