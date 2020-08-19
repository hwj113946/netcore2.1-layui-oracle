using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace WebApplication1.Models.ApiModels
{
    public class JsonContent : StringContent
    {
        public JsonContent(object obj) :
        base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        { }
    }

}
