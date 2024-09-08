using DurableTask.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TestFunction.Models;
using Microsoft.Azure.WebJobs.Extensions.Sql;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using MoreLinq;
using Azure;

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
        public IActionResult GetEmployee([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "employees/{employeeNum}")] HttpRequest req, [FromRoute] int employeeNum)
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
        public IActionResult PostEmployee([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "employees")] HttpRequest req, [FromBody] string name, [FromBody] string occupation)
        {
            if (Employees.Any(x => x.Name.Equals(name)))
            {
                return new BadRequestObjectResult("Employee with that name already exists");
            }

            var postEmployee = new Employee(name, occupation);
            Employees.Add(postEmployee);
            _logger.LogInformation($"Employee: {name} has been added to the list!");
            return new CreatedResult(nameof(PostEmployee), postEmployee);
        }

        [Function("PutEmployee")]
        public IActionResult PutEmployee([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "employees/{employeeNum}")] HttpRequest req, [FromRoute] int employeeNum, [FromBody] string name, [FromBody] string occupation)
        {
            try
            {
                var putEmployee = new Employee(name, occupation);
                var employeeIdx = Employees.IndexOf(Employees[employeeNum - 1]);

                Employees.Insert(employeeIdx, putEmployee);
                return new OkObjectResult($"{putEmployee.Name} has been updated!");
            }

            catch (Exception e)
            {
                return new BadRequestObjectResult(e.ToString());
            }
            
        }

        [Function("DeleteEmployee")]
        public IActionResult DeleteEmployee([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "employees/{employeeNum}")] HttpRequest req, [FromRoute] int employeeNum)
        {
            try
            {
                var deleteEmployee = Employees[employeeNum - 1];
                var deleted = Employees.Remove(deleteEmployee);

                if (!deleted)
                {
                    return new BadRequestObjectResult($"Error deleting: {deleteEmployee.Name} deleted");
                }

                return new OkObjectResult($"Employee: {deleteEmployee.Name} has been deleted. There are now: {Employees.Count()} employees left!");
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

        [Function("AzureSQLTrigger")]
        public async Task AzureSQLTrigger(
            [SqlTrigger("[dbo].[Employee]", "A_Connection_String")]
            IReadOnlyList<Microsoft.Azure.WebJobs.Extensions.Sql.SqlChange<Employee>> dbChanges,
            FunctionContext context)
        {
            var logger = context.GetLogger("NewTrigger");
            logger.LogInformation($"There has beeen {dbChanges.Count()} changes in the database!");
            _logger.LogInformation("Here are the changes...");

            await Task.Delay(2000);

            var insertChanges = dbChanges.ToList().Where(chg => chg.Operation.ToString().Equals("Insert")).Count();
            var updateChanges = dbChanges.ToList().Where(chg => chg.Operation.ToString().Equals("Update")).Count();
            var deleteChanges = dbChanges.ToList().Where(chg => chg.Operation.ToString().Equals("Delete")).Count();

            var modifiedOpsSet = new Dictionary<string, dynamic>
            {
                ["Inserted"] = insertChanges > 0 ? insertChanges : "No Changes",
                ["Updated"] = updateChanges > 0 ? updateChanges : "No Changes",
                ["Deleted"] = deleteChanges > 0 ? deleteChanges : "No Changes"
            };

            modifiedOpsSet.ForEach(x => Console.WriteLine($"{x.Key} - {x.Value}"));
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
