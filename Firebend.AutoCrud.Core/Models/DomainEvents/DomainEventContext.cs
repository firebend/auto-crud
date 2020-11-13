using Newtonsoft.Json;

namespace Firebend.AutoCrud.Core.Models.DomainEvents
{
    public class DomainEventContext
    {
        private string _customContextJson;
        public string UserEmail { get; set; }

        public string Source { get; set; }

        public object CustomContext
        {
            get => _customContextJson == null ? null : JsonConvert.DeserializeObject(_customContextJson);
            set
            {
                if (value == null)
                {
                    _customContextJson = null;
                }
                else
                {
                    _customContextJson = JsonConvert.SerializeObject(value);
                }
            }
        }

        public T GetCustomContext<T>()
        {
            if (string.IsNullOrWhiteSpace(_customContextJson))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(_customContextJson);
        }
    }
}
