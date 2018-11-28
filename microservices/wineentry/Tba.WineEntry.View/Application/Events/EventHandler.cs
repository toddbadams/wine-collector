using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tba.WineEntry.Configuration;

namespace Tba.WineEntry.View.Application.Events
{
    public static class EventHandler
    {
        public static bool TryDeserializeAndValidate(string message, ILogger log, string correlationId, out Event e)
        {
            e = null;
            try
            {
                e = JsonConvert.DeserializeObject<Event>(message);
                Validator.ValidateObject(e, new ValidationContext(e));
                log.LogInformation(Config.Logging.GetEventId(Config.Logging.EventType.ValidationSucceeded),
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(), correlationId, nameof(Event), e.AggregateId,
                    $"Valid request");
                return true;
            }
            catch (JsonException ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ValidationFailed),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(), correlationId, nameof(Event), e?.AggregateId,
                    $"request can not deserialize");
                return false;
            }
            catch (ArgumentNullException ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ValidationFailed),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(), correlationId, nameof(Event), e?.AggregateId,
                    $"request is null");
                return false;
            }
            catch (ValidationException ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ValidationFailed),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(), correlationId, nameof(Event), e?.AggregateId,
                    $"Validation failed {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ValidationFailed),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(), correlationId, nameof(Event), e?.AggregateId,
                    $"unhandled exception");
                return false;
            }
        }
    }
}