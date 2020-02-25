using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            // send the error message
            response.Headers.Add("Application-Error", message);
            // write cors headers to the output stream so it doesnt throw NO CORS errors on angular page
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            // allow wildcard cors headers so it is allowed
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }
        
        // well return pagination information as headers
        public static void AddPagination(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            // the header needs to be a string, so we convert the paginationheader object to a json string
            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, camelCaseFormatter));
            // write cors headers to the output stream so it doesnt throw NO CORS errors on angular page
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }

        public static int CalculateAge(this DateTime theDateTime)
        {
            var age = DateTime.Today.Year - theDateTime.Year;
            
            // check to see if their birthday has happened yet
            if (theDateTime.AddYears(age) > DateTime.Today)
                age--;
            
            return age;
        }
    }
}