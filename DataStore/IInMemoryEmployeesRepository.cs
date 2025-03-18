using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.DataStore
{
    /// <summary>
    /// This interface is used to manage the employee information in memory.
    /// </summary>
    public interface IInMemoryEmployeesRepository
    {
        /// <summary>
        /// Loads employees from the provided data.
        /// </summary>
        /// <param name="data">The data containing employee information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadEmployees(JToken data);
        /// <summary>
        /// Gets all employees.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of EmployeeInfo.</returns>
        Task<IEnumerable<EmployeeInfo>> GetAll();

        /// <summary>
        /// Resets the employee data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the reset was successful.</returns>
        Task<bool> Reset();

        /// <summary>
        /// Sets up the employee data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the setup was successful.</returns>
        Task<bool> Setup();

        /// <summary>
        /// Loads HECSEmployees from the provided result.
        /// </summary>
        /// <param name="result">The result containing HECSEmployees information.</param>
        /// <param name="stoppingToken">The cancellation token to stop the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the load was successful.</returns>
        Task<bool> LoadHECSEmployees(Hces result, CancellationToken stoppingToken);

        /// <summary>
        /// Loads SMSEmployeeInfo from the provided result.
        /// </summary>
        /// <param name="result">The result containing SMSEmployeeInfo information.</param>
        /// <param name="stoppingToken">The cancellation token to stop the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the load was successful.</returns>
        Task<bool> LoadSMSEmployeeInfo(List<SMSWrapperEmployeeInfo> result, CancellationToken stoppingToken);

        /// <summary>
        /// Updates employee information from EPAC.
        /// </summary>
        /// <param name="epac">The JObject containing EPAC information.</param>
        void UpdateEmployeeInfoFromEPAC(JObject epac);

        /// <summary>
        /// Gets an employee by BLE ID.
        /// </summary>
        /// <param name="id">The BLE ID of the employee.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the EmployeeInfo if found, otherwise null.</returns>
        Task<EmployeeInfo> GetEmployeeByBLE(string id);

        /// <summary>
        /// Gets an employee by EIN.
        /// </summary>
        /// <param name="id">The EIN of the employee.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the employee object.</returns>
        Task<EmployeeInfo> GetEmployeeByEIN(string id);

        /// <summary>
        /// Gets an employee by code.
        /// </summary>
        /// <param name="code">The code of the employee.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the employee object if found, otherwise null.</returns>
        Task<object?> GetEmployeeByCode(string code);

        /// <summary>
        /// Gets a distinct list of employee IDs.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of distinct employee IDs.</returns>
        Task<List<string?>> GetDistinctEmployeeIdList();

        /// <summary>
        /// Searches for employees based on the provided search string.
        /// </summary>
        /// <param name="search">The search string.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of employees matching the search criteria.</returns>
        Task<List<JObject>> SearchEmployee(string search);

        /// <summary>
        /// Gets the list of employees.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of employees.</returns>
        Task<object> GetEmployeesList();

    }
}