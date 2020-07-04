using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DatingAPI.Helpers
{
    public static class ExtensionMethods
    {
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }
        public static int Age(this DateTime theDate)
        {
            var age = (DateTime.Today.Year - theDate.Year);
            if(theDate.AddYears(age) > DateTime.Today )
            {
                age--;
            }
            return age;
        }
        
    }
}
