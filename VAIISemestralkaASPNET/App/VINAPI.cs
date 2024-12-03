using System.Text;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Humanizer;

namespace VAIISemestralkaASPNET.App
{
    public class VINAPI
    {
        public static async Task<VINDetails?> GetVinDetailsAsync(string vin)
        {
            string id = "decode";
            string apiKey = "90ee963016c5";
            string secretKey = "0e9b3217d8";
            string normalizedVIN = ConvertToLatin(vin.ToUpper());

            string input = $"{normalizedVIN}|{id}|{apiKey}|{secretKey}";
            string controlSum = GenerateSha1Hash(input).Substring(0, 10);

            string url = $"https://api.vindecoder.eu/3.2/{apiKey}/{controlSum}/decode/{normalizedVIN}.json";

            using HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseData);

                if (json.TryGetProperty("decode", out var decodeArray))
                {
                    return MapToVINDetails(decodeArray);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public static async Task<IEnumerable<Info>> GetInfos(string vin)
        {
            List<Info> list = new List<Info>();

            try
            {
                VINDetails? details = await GetVinDetailsAsync(vin);

                if (details != null)
                {
                    list.Add(new Info("VIN", details.VIN));
                    list.Add(new Info("Make", details.Make));
                    list.Add(new Info("Model", details.Model));
                    list.Add(new Info("ModelYear", details.ModelYear));
                    list.Add(new Info("Body", details.Body));
                    list.Add(new Info("Drive", details.Drive));
                    list.Add(new Info("EngineDisplacement", details.EngineDisplacement));
                    list.Add(new Info("EnginePowerKW", details.EnginePowerKW));
                    list.Add(new Info("EnginePowerHP", details.EnginePowerHP));
                    list.Add(new Info("FuelType", details.FuelType));
                    list.Add(new Info("Transmission", details.Transmission));
                    list.Add(new Info("NumberOfGears", details.NumberOfGears));
                    list.Add(new Info("EmissionStandard", details.EmissionStandard));
                    list.Add(new Info("MaxSpeed", details.MaxSpeed));
                    list.Add(new Info("Color", details.Color));
                    list.Add(new Info("NumberOfDoors", details.NumberOfDoors));
                    list.Add(new Info("NumberOfSeats", details.NumberOfSeats));
                }
                else
                {
                    list.Add(new Info("NONE", "NONE"));
                }
            }
            catch
            {
                list.Add(new Info("NONE", "NONE"));
            }

            return list;
        }

        private static VINDetails MapToVINDetails(JsonElement decodeArray)
        {
            var details = new VINDetails();

            foreach (var item in decodeArray.EnumerateArray())
            {
                string label = item.GetProperty("label").GetString();
                string value = item.GetProperty("value").ToString();

                switch (label)
                {
                    case "VIN":
                        details.VIN = value;
                        break;
                    case "Make":
                        details.Make = value;
                        break;
                    case "Model":
                        details.Model = value;
                        break;
                    case "Model Year":
                        details.ModelYear = int.Parse(value);
                        break;
                    case "Body":
                        details.Body = value;
                        break;
                    case "Drive":
                        if (value == "4x4 - Four-wheel drive")
                        {
                            details.Drive = "Front - wheel drive";
                        }
                        else if (value == "Front-wheel drive")
                        {
                            details.Drive = "4x4 - Four-wheel drive";
                        }
                        else
                        {
                            details.Drive = value;
                        }
                        break;
                    case "Engine Displacement (ccm)":
                        details.EngineDisplacement = int.Parse(value);
                        break;
                    case "Engine Power (kW)":
                        details.EnginePowerKW = int.Parse(value);
                        break;
                    case "Engine Power (HP)":
                        details.EnginePowerHP = int.Parse(value);
                        break;
                    case "Fuel Type - Primary":
                        details.FuelType = value;
                        break;
                    case "Transmission":
                        details.Transmission = value;
                        break;
                    case "Number of Gears":
                        details.NumberOfGears = int.Parse(value);
                        break;
                    case "Emission Standard":
                        details.EmissionStandard = value;
                        break;
                    case "Max Speed (km/h)":
                        details.MaxSpeed = int.Parse(value);
                        break;
                    case "Color":
                        details.Color = value;
                        break;
                    case "Number of Doors":
                        details.NumberOfDoors = int.Parse(value);
                        break;
                    case "Number of Seats":
                        details.NumberOfSeats = int.Parse(value);
                        break;
                }
            }

            return details;
        }

        private static string ConvertToLatin(string vin)
        {
            var cyrillicToLatinMap = new Dictionary<char, char>
        {
            { 'А', 'A' }, { 'В', 'B' }, { 'Е', 'E' }, { 'К', 'K' },
            { 'М', 'M' }, { 'Н', 'H' }, { 'О', 'O' }, { 'Р', 'P' },
            { 'С', 'S' }, { 'Т', 'T' }, { 'У', 'Y' }, { 'Х', 'X' }
        };

            var builder = new StringBuilder();

            foreach (char c in vin)
            {
                if (cyrillicToLatinMap.ContainsKey(c))
                {
                    builder.Append(cyrillicToLatinMap[c]);
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        private static string GenerateSha1Hash(string input)
        {
            using SHA1 sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public class Info
        {
            public string Name { get; set; }
            public string Values { get; set; }

            public Info()
            {
                Name = String.Empty;
                Values = String.Empty;
            }

            public Info(string name, string values)
            {
                Name = name; ;
                Values = values;
            }

            public Info(string name, int values)
            {
                Name = name; ;
                Values = values.ToString();
            }
        }
    }
}
