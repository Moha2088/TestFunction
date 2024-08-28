using DurableTask.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TestFunction.Models;

namespace TestFunction
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        public static List<Employee> Employees { get; set; } = null!;


        [Function("Welcome")]
        public IActionResult Welcome([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "welcome")] FunctionContext ctx)
        {
            _logger.LogInformation($"Getting Information about this Function, Name: {ctx.FunctionDefinition.Name}\nParameters: {ctx.FunctionDefinition.Parameters}\n");
            _logger.LogInformation($"C# HTTP trigger function processed a request at {DateTime.Now}");
            return new OkObjectResult("Welcome to Azure Functions!");
        }

        [Function("GetEmployees")]
        public IActionResult GetEmployees([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "employees")] HttpRequest req)
        {
            _logger.LogInformation("Getting all employees...");
            return new OkObjectResult(Employees);
        }

        [Function("GetEmployee")]
        public IActionResult GetEmployee([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "employees/{employeeNum}")] [FromRoute] int employeeNum)
        {
            try
            {
                var employee = Employees[employeeNum - 1];
                _logger.LogInformation("Getting employee...");
                return new OkObjectResult(employee);
            }

            catch (ArgumentOutOfRangeException)
            {
                return new NotFoundObjectResult($"No employee was found with ID: {employeeNum}\n\nPlease enter a number from: {Employees.IndexOf(Employees.First()) + 1} - {Employees.IndexOf(Employees.Last()) + 1}");
            }

            catch (Exception e)
            {
                return new BadRequestObjectResult(e.ToString());
            }
        }

        [Function("PostEmployee")]
        public IActionResult PostEmployee([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "employees")] [FromBody] string name, [FromBody] string occupation)
        {
            if(Employees.Any(x => x.Name.Equals(name) && x.Occupation.Equals(occupation)))
            {
                return new BadRequestObjectResult("Employee with that name and occupation already exists");
            }

            var postEmployee = new Employee(name, occupation);
            Employees.Add(postEmployee);
            _logger.LogInformation($"Employee: {name} has been added to the list!");
            return new CreatedResult(nameof(PostEmployee), postEmployee);
        }

        [Function("DeleteEmployee")]
        public IActionResult DeleteEmployee([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "employees/{employeeNum}")] [FromRoute]int employeeNum)
        {
            try
            {
                var deleteEmployee = Employees[employeeNum - 1];
                var deleted = Employees.Remove(deleteEmployee);

                if (!deleted)
                {
                    return new BadRequestObjectResult($"Error deleting: {deleteEmployee.Name} deleted");
                }

                return new OkObjectResult($"Employee: {deleteEmployee.Name} has been deleted");
            }

            catch (ArgumentOutOfRangeException)
            {
                return new NotFoundObjectResult($"No employee was found with ID: {employeeNum}\n\nPlease enter a number from: {Employees.IndexOf(Employees.First()) + 1} - {Employees.IndexOf(Employees.Last()) + 1}");
            }

            catch (Exception ex) 
            {
                return new BadRequestObjectResult(ex.ToString());

            }
        }

        public static void SeedData()
        {
            Employees = new List<Employee>
            {
                new Employee
                {
                    Name = "Sang",
                    Occupation = "Team Lead & Project Manager"
                },

                new Employee
                {
                    Name = "Michael",
                    Occupation = "Backend Developer"
                },

                new Employee
                {
                    Name = "Mohamed",
                    Occupation = "Backend Developer Intern"
                },

                new Employee
                {
                    Name = "Simon",
                    Occupation = "Robotics Engineer"
                },

                new Employee
                {
                    Name = "Nikolaj",
                    Occupation = "Robotics Engineer"
                },

                new Employee
                {
                    Name = "Emil Asgeirsson",
                    Occupation = "Product Manager | Analytics"
                },

                new Employee
                {
                    Name = "Asbjørn",
                    Occupation = "Acoustic Specialist"
                },

                new Employee
                {
                    Name = "Kristian Larsen",
                    Occupation = "Head of Development"
                },

                 new Employee
                 {
                    Name = "Frederik Hex",
                    Occupation = "Head of Marketing",
                 },

                new Employee
                {
                    Name = "Mads Helle",
                    Occupation = "Chief Technology Officer",
                },

                 new Employee
                 {
                    Name = "Gert Nielsen",
                    Occupation = "Chief Executive Officer",
                 },
            };
        }
    }
}
