using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using AutoDynamics.Shared.Services;
using AutoDynamics.Shared.Modals.ComponentsTypes;
using System.Diagnostics;
namespace AutoDynamics.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<ThirukkuralType> GetQuoteOfTheDay()
        {
            try
            {
                // Valid kural numbers range — your logic
                int[] validKurals = Enumerable.Range(1, 380) // Covers Aram and part of Porul
                       .Concat(Enumerable.Range(381, 700)) // Excludes 1081-1330
                       .ToArray();

                int randomKuralNumber = validKurals[new Random().Next(validKurals.Length)];


                // Path to JSON in Windows output folder
                string baseDirectory = AppContext.BaseDirectory;

                // Calculate the path to thirukkural.json in the wwwroot/files directory
                string filePath = Path.Combine(baseDirectory,"wwwroot", "files", "thirukkural.json");

                Debug.WriteLine(filePath);
                // Read and deserialize
                var json = await File.ReadAllTextAsync(filePath);
                
                var thirukkuralData = JsonSerializer.Deserialize<ThirukkuralRoot>(json);
                Debug.WriteLine(thirukkuralData.kural[0].mk);
                // Get the list of kurrals
                var kurrals = thirukkuralData?.kural;
                
                if (kurrals != null)
                {
                    // Pick a random Thirukkural (or based on the Number as needed)
                    var selectedKural = kurrals.FirstOrDefault(k => k.Number == randomKuralNumber);
                    Debug.WriteLine(selectedKural);
                    return new ThirukkuralType{
                        kural = selectedKural != null ? new string[] { selectedKural.Line1, selectedKural.Line2 } : new string[] { "", "" },
                        athikaram = selectedKural?.adikaram_name ?? "",
                        KurralNo = randomKuralNumber,
                        iyal = selectedKural?.iyal_name ?? "",
                        Pall = selectedKural?.paul_name ?? "", // Ensure correct property name
                        vilakam = selectedKural?.mk ?? ""
                    }
                    ;
                }

                return new ThirukkuralType();
            }
            catch
            {
                return new ThirukkuralType();
            }
        }



    }

}

