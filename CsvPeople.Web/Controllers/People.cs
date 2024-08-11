using CsvHelper;
using CsvHelper.Configuration;
using CsvPeople.Data;
using CsvPeople.Web.Models;
using Faker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;

namespace CsvPeople.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class People : ControllerBase
    {
        private readonly string _connectionString;

        public People(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConStr");

        }

        [HttpPost("addpeople")]
        public void AddPeople(UploadViewModel vm)
        {
            var repo = new PersonRepo(_connectionString);
            int indexOfComma = vm.Base64Data.IndexOf(',');
            string base64 = vm.Base64Data.Substring(indexOfComma + 1);
            byte[] csvBytes = Convert.FromBase64String(base64);
            List<Person> people = GetFromCsvBytes(csvBytes);
            repo.UploadPeople(people);
        }

        [HttpGet]
        [Route("getall")]
        public List<Person> GetAll()
        {
            var repo = new PersonRepo(_connectionString);
            return repo.GetAllPeople();
        }

        [HttpPost]
        [Route("deleteall")]
        public void DeleteAll()
        {
            var repo = new PersonRepo(_connectionString);
            repo.DeleteAll();
        }


        [HttpGet("generate")]
        public IActionResult Generate(int amount)
        {
            List<Person> ppl = Enumerable.Range(1, amount).Select(_ => new Person
            {
                FirstName = Name.First(),
                LastName = Name.Last(),
                Age = RandomNumber.Next(20, 80),
                Address = Address.StreetAddress(),
                Email = Internet.Email()
            }).ToList();

            string csv = GenerateCsv(ppl);

            byte[] csvBytes = Encoding.UTF8.GetBytes(csv);
            return File(csvBytes, "text/csv", "people.csv");
        }

        private static string GenerateCsv(List<Person> people)
        {
            var writer = new StringWriter();
            var csvWriter = new CsvHelper.CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
            csvWriter.WriteRecords(people);

            return writer.ToString();
        }
        [HttpGet("getGeneratedPeople")]
        public IActionResult GetGeneratedPeople()
        {
            return File(System.IO.File.ReadAllBytes("GeneratedPeople/people"), "application/octet-stream", "people.csv");
        }
        static List<Person> GetFromCsvBytes(byte[] csvBytes)
        {
            using var memoryStream = new MemoryStream(csvBytes);
            using var reader = new StreamReader(memoryStream);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            return csvReader.GetRecords<Person>().ToList();
        }

      
     

    }
}
