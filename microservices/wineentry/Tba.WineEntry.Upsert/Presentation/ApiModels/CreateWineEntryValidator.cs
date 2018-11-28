using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Tba.WineEntry.Configuration;

namespace Tba.WineEntry.Upsert.Presentation.ApiModels
{
    public class CreateWineEntryValidator
    {
        public bool TryDeserializeAndValidate(string requestBody, out CreateWineEntryRequest requestModel, out string message)
        {
            requestModel = null;
            try
            {
                requestModel = JsonConvert.DeserializeObject<CreateWineEntryRequest>(requestBody);
                Validator.ValidateObject(requestModel, new ValidationContext(requestModel));
                message = Config.Http.ValidationMessage.Success.ToString();
                return true;
            }
            catch (ArgumentNullException)
            {
                message = Config.Http.ValidationMessage.Null.ToString();
            }
            catch (ValidationException ex)
            {
                message = $"{Config.Http.ValidationMessage.Invalid} {ex.Message}";
            }
            catch (Exception)
            {
                message = Config.Http.ValidationMessage.Unhandled.ToString();
            }

            return false;
        }
    }
}