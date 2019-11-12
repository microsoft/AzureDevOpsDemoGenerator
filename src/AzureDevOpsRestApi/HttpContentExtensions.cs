using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


public static class HttpContentExtensions
{
    public static async Task<T> ReadAsAsync<T>(this HttpContent content)
    {
        string json = await content.ReadAsStringAsync();
        T value = JsonConvert.DeserializeObject<T>(json);
        return value;
    }
}

