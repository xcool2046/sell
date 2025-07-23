using Sellsys.WpfClient.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

namespace Sellsys.WpfClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string BaseUrl;

        // 配置不同环境的API地址
        private const string DevelopmentUrl = "http://localhost:5078/api";
        private const string ProductionUrl = "http://8.156.69.42:5000/api";

        public ApiService()
        {
            // 自动检测环境：如果是Debug模式使用开发环境，否则使用生产环境
#if DEBUG
            BaseUrl = DevelopmentUrl;
            System.Diagnostics.Debug.WriteLine($"ApiService: 使用开发环境 - {BaseUrl}");
#else
            BaseUrl = ProductionUrl;
            System.Diagnostics.Debug.WriteLine($"ApiService: 使用生产环境 - {BaseUrl}");
#endif

            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Increase timeout for better reliability
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
        }

        public async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"API连接失败 - 网络错误: {ex.Message}");
                return false;
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"API连接失败 - 请求超时: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API连接失败 - 未知错误: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool IsAvailable, string ErrorMessage)> CheckApiConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/health");
                if (response.IsSuccessStatusCode)
                {
                    return (true, "连接成功");
                }
                else
                {
                    return (false, $"服务器返回错误状态码: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"网络连接失败: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                return (false, $"请求超时: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"连接失败: {ex.Message}");
            }
        }

        public async Task<List<Product>?> GetProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Product>>>($"{BaseUrl}/products");
                if (response != null && response.IsSuccess)
                {
                    return response.Data;
                }
                // TODO: Log error or handle unsuccessful response
                return null;
            }
            catch (Exception ex)
            {
                // For now, we can just output the error to the console for debugging.
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<Product>>($"{BaseUrl}/products/{productId}");
                if (response != null && response.IsSuccess)
                {
                    return response.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task<Product> CreateProductAsync(ProductUpsertDto productDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/products", productDto);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Product>>();
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
                throw new Exception(apiResponse?.Message ?? "创建产品失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task UpdateProductAsync(int productId, ProductUpsertDto productDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/products/{productId}", productDto);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse != null && !apiResponse.IsSuccess)
                {
                    throw new Exception(apiResponse.Message ?? "更新产品失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/products/{productId}");

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse != null && !apiResponse.IsSuccess)
                {
                    throw new Exception(apiResponse.Message ?? "删除产品失败");
                }

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        // Customer API methods
        public async Task<List<Customer>> GetCustomersAsync()
        {
            try
            {
                var httpResponse = await _httpClient.GetAsync($"{BaseUrl}/customers");
                httpResponse.EnsureSuccessStatusCode();

                var content = await httpResponse.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new Exception("服务器返回空响应");
                }

                var response = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<List<CustomerDto>>>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToCustomer).ToList();
                }
                throw new Exception(response?.Message ?? "获取客户列表失败");
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取客户列表 - 网络请求失败: {ex.Message}");
                throw new Exception($"网络连接失败，请检查服务器是否运行: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取客户列表 - 请求超时: {ex.Message}");
                throw new Exception($"请求超时，请稍后重试: {ex.Message}");
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取客户列表 - 数据解析失败: {ex.Message}");
                throw new Exception($"服务器返回数据格式错误: {ex.Message}");
            }
            catch (Exception ex) when (!(ex.Message.Contains("获取客户列表失败")))
            {
                System.Diagnostics.Debug.WriteLine($"获取客户列表 - 未知错误: {ex.Message}");
                throw new Exception($"获取客户列表时发生未知错误: {ex.Message}");
            }
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<CustomerDto>>($"{BaseUrl}/customers/{id}");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return MapToCustomer(response.Data);
                }
                throw new Exception(response?.Message ?? "获取客户信息失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<Customer> CreateCustomerAsync(CustomerUpsertDto customerDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/customers", customerDto);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new Exception("服务器返回空响应");
                }

                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<CustomerDto>>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return MapToCustomer(apiResponse.Data);
                }
                throw new Exception(apiResponse?.Message ?? "创建客户失败");
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建客户 - 网络请求失败: {ex.Message}");
                throw new Exception($"网络连接失败，无法创建客户，请检查服务器连接: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建客户 - 请求超时: {ex.Message}");
                throw new Exception($"创建客户请求超时，请稍后重试: {ex.Message}");
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建客户 - 数据解析失败: {ex.Message}");
                throw new Exception($"服务器返回数据格式错误: {ex.Message}");
            }
            catch (Exception ex) when (!(ex.Message.Contains("创建客户失败")))
            {
                System.Diagnostics.Debug.WriteLine($"创建客户 - 未知错误: {ex.Message}");
                throw new Exception($"创建客户时发生未知错误: {ex.Message}");
            }
        }

        public async Task UpdateCustomerAsync(int id, CustomerUpsertDto customerDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/customers/{id}", customerDto);
                response.EnsureSuccessStatusCode();

                // PUT requests may return empty content (204 No Content)
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(content, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });

                    if (apiResponse?.IsSuccess != true)
                    {
                        throw new Exception(apiResponse?.Message ?? "更新客户失败");
                    }
                }
                // If content is empty, assume success (HTTP 204)
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新客户 - 网络请求失败: {ex.Message}");
                throw new Exception($"网络连接失败，无法更新客户，请检查服务器连接: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新客户 - 请求超时: {ex.Message}");
                throw new Exception($"更新客户请求超时，请稍后重试: {ex.Message}");
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新客户 - 数据解析失败: {ex.Message}");
                throw new Exception($"服务器返回数据格式错误: {ex.Message}");
            }
            catch (Exception ex) when (!(ex.Message.Contains("更新客户失败")))
            {
                System.Diagnostics.Debug.WriteLine($"更新客户 - 未知错误: {ex.Message}");
                throw new Exception($"更新客户时发生未知错误: {ex.Message}");
            }
        }

        public async Task DeleteCustomerAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/customers/{id}");
                response.EnsureSuccessStatusCode();

                // DELETE requests may return empty content (204 No Content)
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(content, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });

                    if (apiResponse?.IsSuccess != true)
                    {
                        throw new Exception(apiResponse?.Message ?? "删除客户失败");
                    }
                }
                // If content is empty, assume success (HTTP 204)
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"删除客户 - 网络请求失败: {ex.Message}");
                throw new Exception($"网络连接失败，无法删除客户，请检查服务器连接: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"删除客户 - 请求超时: {ex.Message}");
                throw new Exception($"删除客户请求超时，请稍后重试: {ex.Message}");
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"删除客户 - 数据解析失败: {ex.Message}");
                throw new Exception($"服务器返回数据格式错误: {ex.Message}");
            }
            catch (Exception ex) when (!(ex.Message.Contains("删除客户失败")))
            {
                System.Diagnostics.Debug.WriteLine($"删除客户 - 未知错误: {ex.Message}");
                throw new Exception($"删除客户时发生未知错误: {ex.Message}");
            }
        }

        // Customer assignment methods
        public async Task AssignSalesPersonAsync(int customerId, int salesPersonId)
        {
            try
            {
                // Get current customer data
                var customer = await GetCustomerByIdAsync(customerId);

                // Create update DTO with new sales person assignment
                var updateDto = new CustomerUpsertDto
                {
                    Name = customer.Name,
                    Province = customer.Province,
                    City = customer.City,
                    Address = customer.Address,
                    Remarks = customer.Remarks,
                    IndustryTypes = customer.IndustryTypes,
                    SalesPersonId = salesPersonId,
                    SupportPersonId = customer.SupportPersonId,
                    Contacts = customer.Contacts.Select(c => new ContactUpsertDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Phone = c.Phone,
                        IsPrimary = c.IsPrimary
                    }).ToList()
                };

                await UpdateCustomerAsync(customerId, updateDto);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task AssignSupportPersonAsync(int customerId, int supportPersonId)
        {
            try
            {
                // Get current customer data
                var customer = await GetCustomerByIdAsync(customerId);

                // Create update DTO with new support person assignment
                var updateDto = new CustomerUpsertDto
                {
                    Name = customer.Name,
                    Province = customer.Province,
                    City = customer.City,
                    Address = customer.Address,
                    Remarks = customer.Remarks,
                    IndustryTypes = customer.IndustryTypes,
                    SalesPersonId = customer.SalesPersonId,
                    SupportPersonId = supportPersonId,
                    Contacts = customer.Contacts.Select(c => new ContactUpsertDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Phone = c.Phone,
                        IsPrimary = c.IsPrimary
                    }).ToList()
                };

                await UpdateCustomerAsync(customerId, updateDto);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        // Sales Follow-up API methods
        public async Task<List<SalesFollowUpLog>> GetSalesFollowUpLogsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<SalesFollowUpLogDto>>>($"{BaseUrl}/salesfollowuplogs");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToSalesFollowUpLog).ToList();
                }
                throw new Exception(response?.Message ?? "获取销售跟进记录失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<SalesFollowUpLog> CreateSalesFollowUpLogAsync(SalesFollowUpLogUpsertDto logDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/salesfollowuplogs", logDto);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<SalesFollowUpLogDto>>();
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return MapToSalesFollowUpLog(apiResponse.Data);
                }
                throw new Exception(apiResponse?.Message ?? "创建销售跟进记录失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task UpdateSalesFollowUpLogAsync(int id, SalesFollowUpLogUpsertDto logDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/salesfollowuplogs/{id}", logDto);
                response.EnsureSuccessStatusCode();

                // Check if response has content before trying to parse JSON
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    if (apiResponse?.IsSuccess != true)
                    {
                        throw new Exception(apiResponse?.Message ?? "更新销售跟进记录失败");
                    }
                }
                // If no content (204 No Content), consider it successful
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<List<SalesFollowUpLog>> GetSalesFollowUpLogsByCustomerIdAsync(int customerId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<SalesFollowUpLogDto>>>($"{BaseUrl}/salesfollowuplogs/customer/{customerId}");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToSalesFollowUpLog).ToList();
                }
                throw new Exception(response?.Message ?? "获取客户销售跟进记录失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task DeleteSalesFollowUpLogAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/salesfollowuplogs/{id}");
                response.EnsureSuccessStatusCode();

                // Check if response has content before trying to parse JSON
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    if (apiResponse?.IsSuccess != true)
                    {
                        throw new Exception(apiResponse?.Message ?? "删除销售跟进记录失败");
                    }
                }
                // If no content (204 No Content), consider it successful
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        // Authentication API methods
        public async Task<LoginResponseDto?> LoginAsync(string username, string password)
        {
            try
            {
                var loginRequest = new LoginRequestDto
                {
                    Username = username,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/login", loginRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"登录失败: {errorContent}");
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }

                throw new Exception(apiResponse?.Message ?? "登录失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        // Employee API methods
        public async Task<List<Employee>> GetEmployeesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<EmployeeDto>>>($"{BaseUrl}/employees");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToEmployee).ToList();
                }
                throw new Exception(response?.Message ?? "获取员工列表失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<List<Employee>> GetEmployeesByDepartmentAsync(string departmentName)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<EmployeeDto>>>($"{BaseUrl}/employees/by-department/{Uri.EscapeDataString(departmentName)}");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToEmployee).ToList();
                }
                throw new Exception(response?.Message ?? "获取部门员工列表失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<EmployeeDto>>($"{BaseUrl}/employees/{id}");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return MapToEmployee(response.Data);
                }
                throw new Exception(response?.Message ?? "获取员工信息失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<Employee> CreateEmployeeAsync(EmployeeUpsertDto employeeDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/employees", employeeDto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"创建员工失败 (状态码: {response.StatusCode}): {errorContent}");
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return MapToEmployee(apiResponse.Data);
                }
                throw new Exception(apiResponse?.Message ?? "创建员工失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task UpdateEmployeeAsync(int id, EmployeeUpsertDto employeeDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/employees/{id}", employeeDto);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "更新员工失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/employees/{id}");
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "删除员工失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        // Order API methods
        public async Task<List<Order>> GetOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<OrderDto>>>($"{BaseUrl}/orders");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToOrder).ToList();
                }
                throw new Exception(response?.Message ?? "获取订单列表失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<OrderDto>>($"{BaseUrl}/orders/{id}");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return MapToOrder(response.Data);
                }
                throw new Exception(response?.Message ?? "获取订单信息失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<Order> CreateOrderAsync(OrderUpsertDto orderDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/orders", orderDto);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return MapToOrder(apiResponse.Data);
                }
                throw new Exception(apiResponse?.Message ?? "创建订单失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task UpdateOrderAsync(int id, OrderUpsertDto orderDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/orders/{id}", orderDto);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "更新订单失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task DeleteOrderAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/orders/{id}");
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "删除订单失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        // 搜索和筛选订单
        public async Task<List<Order>> SearchOrdersAsync(OrderSearchCriteria criteria)
        {
            try
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(criteria.CustomerName))
                    queryParams.Add($"customerName={Uri.EscapeDataString(criteria.CustomerName)}");

                if (!string.IsNullOrWhiteSpace(criteria.ProductName))
                    queryParams.Add($"productName={Uri.EscapeDataString(criteria.ProductName)}");

                if (criteria.EffectiveDateFrom.HasValue)
                    queryParams.Add($"effectiveDateFrom={criteria.EffectiveDateFrom.Value:yyyy-MM-dd}");

                if (criteria.EffectiveDateTo.HasValue)
                    queryParams.Add($"effectiveDateTo={criteria.EffectiveDateTo.Value:yyyy-MM-dd}");

                if (criteria.ExpiryDateFrom.HasValue)
                    queryParams.Add($"expiryDateFrom={criteria.ExpiryDateFrom.Value:yyyy-MM-dd}");

                if (criteria.ExpiryDateTo.HasValue)
                    queryParams.Add($"expiryDateTo={criteria.ExpiryDateTo.Value:yyyy-MM-dd}");

                if (criteria.CreatedDateFrom.HasValue)
                    queryParams.Add($"createdDateFrom={criteria.CreatedDateFrom.Value:yyyy-MM-dd}");

                if (criteria.CreatedDateTo.HasValue)
                    queryParams.Add($"createdDateTo={criteria.CreatedDateTo.Value:yyyy-MM-dd}");

                if (!string.IsNullOrWhiteSpace(criteria.Status))
                    queryParams.Add($"status={Uri.EscapeDataString(criteria.Status)}");

                if (criteria.SalesPersonId.HasValue)
                    queryParams.Add($"salesPersonId={criteria.SalesPersonId.Value}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var url = $"{BaseUrl}/orders/search{queryString}";

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<OrderDto>>>(url);
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToOrder).ToList();
                }
                throw new Exception(response?.Message ?? "搜索订单失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        // 获取订单统计信息
        public async Task<OrderSummary> GetOrderSummaryAsync(List<int>? orderIds = null)
        {
            try
            {
                var url = $"{BaseUrl}/orders/summary";
                if (orderIds != null && orderIds.Count > 0)
                {
                    var idsParam = string.Join(",", orderIds);
                    url += $"?orderIds={idsParam}";
                }

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<OrderSummaryDto>>(url);
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return MapToOrderSummary(response.Data);
                }
                throw new Exception(response?.Message ?? "获取订单统计失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        // After Sales API methods
        public async Task<List<CustomerAfterSales>> GetCustomersWithAfterSalesInfoAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<CustomerAfterSalesDto>>>($"{BaseUrl}/aftersales/customers");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToCustomerAfterSales).ToList();
                }
                throw new Exception(response?.Message ?? "获取客户售后信息失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<List<CustomerAfterSales>> SearchCustomersWithAfterSalesInfoAsync(
            string? customerName = null,
            string? supportPersonName = null,
            string? status = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(customerName))
                    queryParams.Add($"customerName={Uri.EscapeDataString(customerName)}");
                if (!string.IsNullOrWhiteSpace(supportPersonName))
                    queryParams.Add($"supportPersonName={Uri.EscapeDataString(supportPersonName)}");
                if (!string.IsNullOrWhiteSpace(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<CustomerAfterSalesDto>>>($"{BaseUrl}/aftersales/customers/search{queryString}");

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToCustomerAfterSales).ToList();
                }
                throw new Exception(response?.Message ?? "搜索客户售后信息失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<List<AfterSalesRecord>> GetAfterSalesRecordsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<AfterSalesRecordDto>>>($"{BaseUrl}/aftersales");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToAfterSalesRecord).ToList();
                }
                throw new Exception(response?.Message ?? "获取售后记录失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<AfterSalesRecord> GetAfterSalesRecordByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<AfterSalesRecordDto>>($"{BaseUrl}/aftersales/{id}");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return MapToAfterSalesRecord(response.Data);
                }
                throw new Exception(response?.Message ?? "获取售后记录信息失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<List<AfterSalesRecord>> GetAfterSalesRecordsByCustomerIdAsync(int customerId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<AfterSalesRecordDto>>>($"{BaseUrl}/aftersales/customer/{customerId}");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToAfterSalesRecord).ToList();
                }
                throw new Exception(response?.Message ?? "获取客户售后记录失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<List<AfterSalesRecord>> SearchAfterSalesRecordsAsync(
            string? customerName = null,
            string? province = null,
            string? city = null,
            string? status = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(customerName))
                    queryParams.Add($"customerName={Uri.EscapeDataString(customerName)}");
                if (!string.IsNullOrWhiteSpace(province))
                    queryParams.Add($"province={Uri.EscapeDataString(province)}");
                if (!string.IsNullOrWhiteSpace(city))
                    queryParams.Add($"city={Uri.EscapeDataString(city)}");
                if (!string.IsNullOrWhiteSpace(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var url = $"{BaseUrl}/aftersales/search{queryString}";

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<AfterSalesRecordDto>>>(url);
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToAfterSalesRecord).ToList();
                }
                throw new Exception(response?.Message ?? "搜索售后记录失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<AfterSalesRecord> CreateAfterSalesRecordAsync(AfterSalesRecordUpsertDto recordDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/aftersales", recordDto);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<AfterSalesRecordDto>>();
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return MapToAfterSalesRecord(apiResponse.Data);
                }
                throw new Exception(apiResponse?.Message ?? "创建售后记录失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task UpdateAfterSalesRecordAsync(int id, AfterSalesRecordUpsertDto recordDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/aftersales/{id}", recordDto);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "更新售后记录失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task DeleteAfterSalesRecordAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/aftersales/{id}");
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "删除售后记录失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        // Finance API methods - 新的财务管理方法
        /// <summary>
        /// 获取财务订单明细列表
        /// </summary>
        public async Task<FinanceOrderDetailList> GetFinanceOrderDetailsAsync(FinanceFilter filter)
        {
            try
            {
                var filterDto = new FinanceFilterDto
                {
                    CustomerId = filter.CustomerId,
                    ProductId = filter.ProductId,
                    EffectiveDateStart = filter.EffectiveDateStart,
                    EffectiveDateEnd = filter.EffectiveDateEnd,
                    ExpiryDateStart = filter.ExpiryDateStart,
                    ExpiryDateEnd = filter.ExpiryDateEnd,
                    PaymentDateStart = filter.PaymentDateStart,
                    PaymentDateEnd = filter.PaymentDateEnd,
                    SalesPersonId = filter.SalesPersonId,
                    OrderStatus = filter.OrderStatus,
                    SearchKeyword = filter.SearchKeyword,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/finance/order-details", filterDto);
                var jsonResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FinanceOrderDetailListDto>>();

                if (jsonResponse != null && jsonResponse.IsSuccess && jsonResponse.Data != null)
                {
                    return MapToFinanceOrderDetailList(jsonResponse.Data);
                }
                throw new Exception(jsonResponse?.Message ?? "获取财务订单明细失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取筛选数据源
        /// </summary>
        public async Task<FinanceFilterOptions> GetFinanceFilterOptionsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<FinanceFilterOptionsDto>>($"{BaseUrl}/finance/filter-options");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return MapToFinanceFilterOptions(response.Data);
                }
                throw new Exception(response?.Message ?? "获取筛选数据源失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新收款信息
        /// </summary>
        public async Task UpdatePaymentInfoAsync(UpdatePaymentInfo updateInfo)
        {
            try
            {
                var updateDto = new UpdatePaymentInfoDto
                {
                    OrderId = updateInfo.OrderId,
                    ReceivedAmount = updateInfo.ReceivedAmount,
                    PaymentReceivedDate = updateInfo.PaymentReceivedDate,
                    Remarks = updateInfo.Remarks
                };

                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/finance/payment-info", updateDto);
                var jsonResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FinanceOperationResultDto>>();

                if (jsonResponse == null || !jsonResponse.IsSuccess)
                {
                    throw new Exception(jsonResponse?.Message ?? "更新收款信息失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取财务汇总信息
        /// </summary>
        public async Task<FinanceOrderSummary> GetFinanceSummaryAsync(FinanceFilter filter)
        {
            try
            {
                var filterDto = new FinanceFilterDto
                {
                    CustomerId = filter.CustomerId,
                    ProductId = filter.ProductId,
                    EffectiveDateStart = filter.EffectiveDateStart,
                    EffectiveDateEnd = filter.EffectiveDateEnd,
                    ExpiryDateStart = filter.ExpiryDateStart,
                    ExpiryDateEnd = filter.ExpiryDateEnd,
                    PaymentDateStart = filter.PaymentDateStart,
                    PaymentDateEnd = filter.PaymentDateEnd,
                    SalesPersonId = filter.SalesPersonId,
                    OrderStatus = filter.OrderStatus,
                    SearchKeyword = filter.SearchKeyword
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/finance/summary", filterDto);
                var jsonResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FinanceOrderSummaryDto>>();

                if (jsonResponse != null && jsonResponse.IsSuccess && jsonResponse.Data != null)
                {
                    return MapToFinanceOrderSummary(jsonResponse.Data);
                }
                throw new Exception(jsonResponse?.Message ?? "获取财务汇总失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        // 保留原有方法以兼容现有代码
        public async Task<List<FinanceOrder>> GetFinanceOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<FinanceOrderDto>>>($"{BaseUrl}/finance/orders");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToFinanceOrder).ToList();
                }
                throw new Exception(response?.Message ?? "获取财务订单列表失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<FinanceSummary> GetFinanceSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<FinanceSummaryDto>>($"{BaseUrl}/finance/summary{queryString}");

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return MapToFinanceSummary(response.Data);
                }
                throw new Exception(response?.Message ?? "获取财务汇总失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<List<MonthlySalesData>> GetMonthlySalesDataAsync(int year)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<MonthlySalesDataDto>>>($"{BaseUrl}/finance/monthly-sales/{year}");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToMonthlySalesData).ToList();
                }
                throw new Exception(response?.Message ?? "获取月度销售数据失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<List<SalesPersonCommission>> GetSalesPersonCommissionsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<SalesPersonCommissionDto>>>($"{BaseUrl}/finance/commissions");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToSalesPersonCommission).ToList();
                }
                throw new Exception(response?.Message ?? "获取销售提成数据失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task ConfirmPaymentAsync(int orderId, DateTime paymentDate)
        {
            try
            {
                var requestData = new { PaymentDate = paymentDate };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/finance/orders/{orderId}/confirm-payment", requestData);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "确认收款失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task PayCommissionAsync(int orderId, DateTime paymentDate)
        {
            try
            {
                var requestData = new { PaymentDate = paymentDate };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/finance/orders/{orderId}/pay-commission", requestData);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "支付提成失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        // Department API methods
        public async Task<List<Department>> GetDepartmentsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<DepartmentDto>>>($"{BaseUrl}/departments");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToDepartment).ToList();
                }
                throw new Exception(response?.Message ?? "获取部门列表失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<Department> CreateDepartmentAsync(string departmentName)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/departments", departmentName);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<DepartmentDto>>();
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return MapToDepartment(apiResponse.Data);
                }
                throw new Exception(apiResponse?.Message ?? "创建部门失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task UpdateDepartmentAsync(int id, DepartmentUpsertDto departmentDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/departments/{id}", departmentDto);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "更新部门失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<DepartmentDeleteResult?> DeleteDepartmentAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/departments/{id}");

                // 读取响应内容，无论状态码如何
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // 成功响应
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<DepartmentDeleteResult>>(content, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.IsSuccess == true)
                    {
                        return apiResponse.Data;
                    }
                    throw new Exception(apiResponse?.Message ?? "删除部门失败");
                }
                else
                {
                    // 错误响应，尝试解析错误信息
                    try
                    {
                        var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<DepartmentDeleteResult>>(content, new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        // 抛出包含后端错误信息的异常
                        throw new Exception(errorResponse?.Message ?? $"删除部门失败 (状态码: {response.StatusCode})");
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        // 如果无法解析JSON，使用默认错误信息
                        throw new Exception($"删除部门失败 (状态码: {response.StatusCode}): {content}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception($"请求超时: {ex.Message}");
            }
        }

        // Department Group API methods
        public async Task<List<DepartmentGroup>> GetDepartmentGroupsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<DepartmentGroupDto>>>($"{BaseUrl}/departmentgroups");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToDepartmentGroup).ToList();
                }
                throw new Exception(response?.Message ?? "获取部门分组列表失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<List<DepartmentGroup>> GetDepartmentGroupsByDepartmentIdAsync(int departmentId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<DepartmentGroupDto>>>($"{BaseUrl}/departmentgroups/by-department/{departmentId}");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToDepartmentGroup).ToList();
                }
                throw new Exception(response?.Message ?? "获取部门分组列表失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<DepartmentGroup> CreateDepartmentGroupAsync(int departmentId, string groupName)
        {
            try
            {
                var request = new { DepartmentId = departmentId, Name = groupName };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/departmentgroups", request);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<DepartmentGroupDto>>();
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return MapToDepartmentGroup(apiResponse.Data);
                }
                throw new Exception(apiResponse?.Message ?? "创建部门分组失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task UpdateDepartmentGroupAsync(int id, DepartmentGroupUpsertDto groupDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/departmentgroups/{id}", groupDto);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "更新部门分组失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task DeleteDepartmentGroupAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/departmentgroups/{id}");
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "删除部门分组失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        // Role API methods
        public async Task<List<Role>> GetRolesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<RoleDto>>>($"{BaseUrl}/roles");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(MapToRole).ToList();
                }
                throw new Exception(response?.Message ?? "获取角色列表失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task<Role> CreateRoleAsync(RoleUpsertDto roleDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/roles", roleDto);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RoleDto>>();
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return MapToRole(apiResponse.Data);
                }
                throw new Exception(apiResponse?.Message ?? "创建角色失败");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task UpdateRoleAsync(int id, RoleUpsertDto roleDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/roles/{id}", roleDto);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "更新角色失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        public async Task DeleteRoleAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/roles/{id}");
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (apiResponse?.IsSuccess != true)
                {
                    throw new Exception(apiResponse?.Message ?? "删除角色失败");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}");
            }
        }

        private static Customer MapToCustomer(CustomerDto dto)
        {
            return new Customer
            {
                Id = dto.Id,
                Name = dto.Name,
                Province = dto.Province,
                City = dto.City,
                Address = dto.Address,
                Remarks = dto.Remarks,
                IndustryTypes = dto.IndustryTypes,
                SalesPersonId = dto.SalesPersonId,
                SalesPersonName = dto.SalesPersonName,
                SupportPersonId = dto.SupportPersonId,
                SupportPersonName = dto.SupportPersonName,
                CreatedAt = dto.CreatedAt,
                Contacts = new System.Collections.ObjectModel.ObservableCollection<Contact>(
                    dto.Contacts.Select(c => new Contact
                    {
                        Id = c.Id,
                        CustomerId = c.CustomerId,
                        Name = c.Name,
                        Phone = c.Phone,
                        IsPrimary = c.IsPrimary,
                        CreatedAt = c.CreatedAt
                    })
                )
            };
        }

        private static SalesFollowUpLog MapToSalesFollowUpLog(SalesFollowUpLogDto dto)
        {
            return new SalesFollowUpLog
            {
                Id = dto.Id,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                ContactId = dto.ContactId,
                ContactName = dto.ContactName,
                Summary = dto.Summary,
                CustomerIntention = dto.CustomerIntention,
                NextFollowUpDate = dto.NextFollowUpDate,
                SalesPersonId = dto.SalesPersonId,
                SalesPersonName = dto.SalesPersonName,
                CreatedAt = dto.CreatedAt
            };
        }

        private static Employee MapToEmployee(EmployeeDto dto)
        {
            return new Employee
            {
                Id = dto.Id,
                Name = dto.Name,
                LoginUsername = dto.LoginUsername,
                Phone = dto.Phone,
                BranchAccount = dto.BranchAccount,
                GroupId = dto.GroupId,
                GroupName = dto.GroupName,
                DepartmentName = dto.DepartmentName,
                RoleId = dto.RoleId,
                RoleName = dto.RoleName,
                CreatedAt = dto.CreatedAt
            };
        }

        private static Order MapToOrder(OrderDto dto)
        {
            return new Order
            {
                Id = dto.Id,
                OrderNumber = dto.OrderNumber,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                EffectiveDate = dto.EffectiveDate,
                ExpiryDate = dto.ExpiryDate,
                Status = dto.Status,
                SalesPersonId = dto.SalesPersonId,
                SalesPersonName = dto.SalesPersonName,
                PaymentReceivedDate = dto.PaymentReceivedDate,
                SalesCommissionAmount = dto.SalesCommissionAmount,
                SupervisorCommissionAmount = dto.SupervisorCommissionAmount,
                ManagerCommissionAmount = dto.ManagerCommissionAmount,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                OrderItems = new System.Collections.ObjectModel.ObservableCollection<OrderItem>(
                    dto.OrderItems.Select(item => new OrderItem
                    {
                        Id = item.Id,
                        OrderId = item.OrderId,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductSpecification = item.ProductSpecification,
                        ProductUnit = item.ProductUnit,
                        ProductPrice = item.ProductPrice,
                        ActualPrice = item.ActualPrice,
                        Quantity = item.Quantity,
                        TotalAmount = item.TotalAmount,
                        CreatedAt = item.CreatedAt
                    })
                )
            };
        }

        private static OrderSummary MapToOrderSummary(OrderSummaryDto dto)
        {
            return new OrderSummary
            {
                TotalOrders = dto.TotalOrders,
                TotalAmount = dto.TotalAmount,
                TotalQuantity = dto.TotalQuantity,
                AverageOrderAmount = dto.AverageOrderAmount,
                StatusCounts = dto.StatusCounts
            };
        }

        private static CustomerAfterSales MapToCustomerAfterSales(CustomerAfterSalesDto dto)
        {
            return new CustomerAfterSales
            {
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                Province = dto.Province,
                City = dto.City,
                ContactName = dto.ContactName,
                ContactPhone = dto.ContactPhone,
                ContactCount = dto.ContactCount,
                SalesPersonId = dto.SalesPersonId,
                SalesPersonName = dto.SalesPersonName,
                SupportPersonId = dto.SupportPersonId,
                SupportPersonName = dto.SupportPersonName,
                ServiceRecordCount = dto.ServiceRecordCount,
                UpdatedAt = dto.UpdatedAt,
                CreatedAt = dto.CreatedAt,
                LatestRecordStatus = dto.LatestRecordStatus,
                PendingRecordCount = dto.PendingRecordCount,
                ProcessingRecordCount = dto.ProcessingRecordCount,
                CompletedRecordCount = dto.CompletedRecordCount
            };
        }

        private static AfterSalesRecord MapToAfterSalesRecord(AfterSalesRecordDto dto)
        {
            return new AfterSalesRecord
            {
                Id = dto.Id,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                CustomerProvince = dto.CustomerProvince,
                CustomerCity = dto.CustomerCity,
                ContactId = dto.ContactId,
                ContactName = dto.ContactName,
                ContactPhone = dto.ContactPhone,
                CustomerFeedback = dto.CustomerFeedback,
                OurReply = dto.OurReply,
                Status = dto.Status,
                ProcessedDate = dto.ProcessedDate,
                SupportPersonId = dto.SupportPersonId,
                SupportPersonName = dto.SupportPersonName,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        private static FinanceOrder MapToFinanceOrder(FinanceOrderDto dto)
        {
            return new FinanceOrder
            {
                Id = dto.Id,
                OrderNumber = dto.OrderNumber,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                TotalAmount = dto.TotalAmount,
                PaymentReceivedDate = dto.PaymentReceivedDate,
                PaymentStatus = dto.PaymentStatus,
                SalesPersonId = dto.SalesPersonId,
                SalesPersonName = dto.SalesPersonName,
                SalesCommissionAmount = dto.SalesCommissionAmount,
                SupervisorCommissionAmount = dto.SupervisorCommissionAmount,
                ManagerCommissionAmount = dto.ManagerCommissionAmount,
                CommissionPaid = dto.CommissionPaid,
                CommissionPaidDate = dto.CommissionPaidDate,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        private static FinanceSummary MapToFinanceSummary(FinanceSummaryDto dto)
        {
            return new FinanceSummary
            {
                TotalOrderAmount = dto.TotalOrderAmount,
                TotalReceivedAmount = dto.TotalReceivedAmount,
                TotalPendingAmount = dto.TotalPendingAmount,
                TotalCommissionAmount = dto.TotalCommissionAmount,
                TotalPaidCommissionAmount = dto.TotalPaidCommissionAmount,
                TotalUnpaidCommissionAmount = dto.TotalUnpaidCommissionAmount,
                TotalOrderCount = dto.TotalOrderCount,
                PaidOrderCount = dto.PaidOrderCount,
                PendingOrderCount = dto.PendingOrderCount,
                OverdueOrderCount = dto.OverdueOrderCount
            };
        }

        // 新的财务管理映射方法
        private static FinanceOrderDetailList MapToFinanceOrderDetailList(FinanceOrderDetailListDto dto)
        {
            return new FinanceOrderDetailList
            {
                Items = dto.Items.Select(MapToFinanceOrderDetail).ToList(),
                TotalCount = dto.TotalCount,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                Summary = MapToFinanceOrderSummary(dto.Summary)
            };
        }

        private static FinanceOrderDetail MapToFinanceOrderDetail(FinanceOrderDetailDto dto)
        {
            return new FinanceOrderDetail
            {
                OrderId = dto.OrderId,
                OrderNumber = dto.OrderNumber,
                OrderItemId = dto.OrderItemId,
                RowNumber = dto.RowNumber,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                ProductSpecification = dto.ProductSpecification,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                TotalAmount = dto.TotalAmount,
                ReceivedAmount = dto.ReceivedAmount,
                UnreceivedAmount = dto.UnreceivedAmount,
                PaymentRatio = dto.PaymentRatio,
                PaymentReceivedDate = dto.PaymentReceivedDate,
                SalesPersonId = dto.SalesPersonId,
                SalesPersonName = dto.SalesPersonName,
                EffectiveDate = dto.EffectiveDate,
                ExpiryDate = dto.ExpiryDate,
                OrderStatus = dto.OrderStatus,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        private static FinanceOrderSummary MapToFinanceOrderSummary(FinanceOrderSummaryDto dto)
        {
            return new FinanceOrderSummary
            {
                TotalAmount = dto.TotalAmount,
                TotalReceivedAmount = dto.TotalReceivedAmount,
                TotalUnreceivedAmount = dto.TotalUnreceivedAmount,
                TotalPaymentRatio = dto.TotalPaymentRatio,
                OrderCount = dto.OrderCount,
                OrderItemCount = dto.OrderItemCount
            };
        }

        private static FinanceFilterOptions MapToFinanceFilterOptions(FinanceFilterOptionsDto dto)
        {
            return new FinanceFilterOptions
            {
                Customers = dto.Customers.Select(MapToFilterOption).ToList(),
                Products = dto.Products.Select(MapToFilterOption).ToList(),
                SalesPersons = dto.SalesPersons.Select(MapToFilterOption).ToList(),
                OrderStatuses = dto.OrderStatuses.Select(MapToFilterOption).ToList(),
                EffectiveDateOptions = dto.EffectiveDateOptions.Select(MapToFilterOption).ToList(),
                ExpiryDateOptions = dto.ExpiryDateOptions.Select(MapToFilterOption).ToList(),
                PaymentDateOptions = dto.PaymentDateOptions.Select(MapToFilterOption).ToList()
            };
        }

        private static FilterOption MapToFilterOption(FilterOptionDto dto)
        {
            return new FilterOption
            {
                Value = dto.Value,
                Text = dto.Text,
                IsSelected = dto.IsSelected
            };
        }

        private static MonthlySalesData MapToMonthlySalesData(MonthlySalesDataDto dto)
        {
            return new MonthlySalesData
            {
                Year = dto.Year,
                Month = dto.Month,
                TotalAmount = dto.TotalAmount,
                ReceivedAmount = dto.ReceivedAmount,
                OrderCount = dto.OrderCount
            };
        }

        private static SalesPersonCommission MapToSalesPersonCommission(SalesPersonCommissionDto dto)
        {
            return new SalesPersonCommission
            {
                SalesPersonId = dto.SalesPersonId,
                SalesPersonName = dto.SalesPersonName,
                TotalSalesAmount = dto.TotalSalesAmount,
                TotalCommissionAmount = dto.TotalCommissionAmount,
                PaidCommissionAmount = dto.PaidCommissionAmount,
                UnpaidCommissionAmount = dto.UnpaidCommissionAmount,
                OrderCount = dto.OrderCount
            };
        }

        private static Department MapToDepartment(DepartmentDto dto)
        {
            return new Department
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = dto.CreatedAt
            };
        }

        private static DepartmentGroup MapToDepartmentGroup(DepartmentGroupDto dto)
        {
            return new DepartmentGroup
            {
                Id = dto.Id,
                Name = dto.Name,
                DepartmentId = dto.DepartmentId,
                CreatedAt = dto.CreatedAt,
                Department = dto.Department != null ? MapToDepartment(dto.Department) : null
            };
        }

        private static Role MapToRole(RoleDto dto)
        {
            return new Role
            {
                Id = dto.Id,
                Name = dto.Name,
                AccessibleModules = dto.AccessibleModules ?? string.Empty,
                CreatedAt = dto.CreatedAt
            };
        }
    }

    // A generic ApiResponse to match the backend's structure
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }
    }

    // Authentication DTO classes
    public class LoginRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? DepartmentName { get; set; }
        public string AccessibleModules { get; set; } = string.Empty;
    }

    // DTO classes for API communication
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? Remarks { get; set; }
        public string? IndustryTypes { get; set; }
        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public int? SupportPersonId { get; set; }
        public string? SupportPersonName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ContactDto> Contacts { get; set; } = new List<ContactDto>();
    }

    public class ContactDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerUpsertDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? Remarks { get; set; }
        public string? IndustryTypes { get; set; }
        public int? SalesPersonId { get; set; }
        public int? SupportPersonId { get; set; }
        public List<ContactUpsertDto> Contacts { get; set; } = new List<ContactUpsertDto>();
    }

    public class ContactUpsertDto
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsPrimary { get; set; }
    }

    // Sales Follow-up DTO classes
    public class SalesFollowUpLogDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }
        public string? Summary { get; set; }
        public string? CustomerIntention { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SalesFollowUpLogUpsertDto
    {
        public int CustomerId { get; set; }
        public int? ContactId { get; set; }
        public string? Summary { get; set; }
        public string? CustomerIntention { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public int? SalesPersonId { get; set; }
    }

    // Employee DTO classes
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LoginUsername { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? BranchAccount { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? DepartmentName { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EmployeeUpsertDto
    {
        public string Name { get; set; } = string.Empty;
        public string LoginUsername { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? BranchAccount { get; set; }
        public int? GroupId { get; set; }
        public int? RoleId { get; set; }
        public string? Password { get; set; }
    }

    // Order DTO classes
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public DateTime? PaymentReceivedDate { get; set; }
        public decimal? SalesCommissionAmount { get; set; }
        public decimal? SupervisorCommissionAmount { get; set; }
        public decimal? ManagerCommissionAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductSpecification { get; set; }
        public string? ProductUnit { get; set; }
        public decimal? ProductPrice { get; set; } // 产品原价
        public decimal ActualPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderUpsertDto
    {
        public int CustomerId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Draft";
        public int SalesPersonId { get; set; }
        public DateTime? PaymentReceivedDate { get; set; }
        public List<OrderItemUpsertDto> OrderItems { get; set; } = new List<OrderItemUpsertDto>();
    }

    public class OrderItemUpsertDto
    {
        public int? Id { get; set; }
        public int ProductId { get; set; }
        public decimal ActualPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // 订单搜索条件
    public class OrderSearchCriteria
    {
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
        public DateTime? EffectiveDateFrom { get; set; }
        public DateTime? EffectiveDateTo { get; set; }
        public DateTime? ExpiryDateFrom { get; set; }
        public DateTime? ExpiryDateTo { get; set; }
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }
        public string? Status { get; set; }
        public int? SalesPersonId { get; set; }
    }

    // 订单统计信息DTO
    public class OrderSummaryDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal AverageOrderAmount { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; } = new Dictionary<string, int>();
    }

    // After Sales DTO classes
    public class CustomerAfterSalesDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public int ContactCount { get; set; }
        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public int? SupportPersonId { get; set; }
        public string? SupportPersonName { get; set; }
        public int ServiceRecordCount { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LatestRecordStatus { get; set; }
        public int PendingRecordCount { get; set; }
        public int ProcessingRecordCount { get; set; }
        public int CompletedRecordCount { get; set; }
    }

    public class AfterSalesRecordDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerProvince { get; set; }
        public string? CustomerCity { get; set; }
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? CustomerFeedback { get; set; }
        public string? OurReply { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedDate { get; set; }
        public int? SupportPersonId { get; set; }
        public string? SupportPersonName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AfterSalesRecordUpsertDto
    {
        public int CustomerId { get; set; }
        public int? ContactId { get; set; }
        public string? CustomerFeedback { get; set; }
        public string? OurReply { get; set; }
        public string Status { get; set; } = "待处理";
        public DateTime? ProcessedDate { get; set; }
        public int? SupportPersonId { get; set; }
    }

    // Finance DTO classes
    public class FinanceOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime? PaymentReceivedDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public int SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public decimal? SalesCommissionAmount { get; set; }
        public decimal? SupervisorCommissionAmount { get; set; }
        public decimal? ManagerCommissionAmount { get; set; }
        public bool CommissionPaid { get; set; }
        public DateTime? CommissionPaidDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FinanceSummaryDto
    {
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalReceivedAmount { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public decimal TotalCommissionAmount { get; set; }
        public decimal TotalPaidCommissionAmount { get; set; }
        public decimal TotalUnpaidCommissionAmount { get; set; }
        public int TotalOrderCount { get; set; }
        public int PaidOrderCount { get; set; }
        public int PendingOrderCount { get; set; }
        public int OverdueOrderCount { get; set; }
    }

    public class MonthlySalesDataDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public int OrderCount { get; set; }
    }

    public class SalesPersonCommissionDto
    {
        public int SalesPersonId { get; set; }
        public string SalesPersonName { get; set; } = string.Empty;
        public decimal TotalSalesAmount { get; set; }
        public decimal TotalCommissionAmount { get; set; }
        public decimal PaidCommissionAmount { get; set; }
        public decimal UnpaidCommissionAmount { get; set; }
        public int OrderCount { get; set; }
    }

    // System Settings DTO classes
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DepartmentUpsertDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class DepartmentGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DepartmentDto? Department { get; set; }
    }

    public class DepartmentGroupUpsertDto
    {
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AccessibleModules { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class RoleUpsertDto
    {
        public string Name { get; set; } = string.Empty;
        public List<string> AccessibleModules { get; set; } = new List<string>();
    }

    // 新的财务管理DTO类
    public class FinanceFilterDto
    {
        public int? CustomerId { get; set; }
        public int? ProductId { get; set; }
        public DateTime? EffectiveDateStart { get; set; }
        public DateTime? EffectiveDateEnd { get; set; }
        public DateTime? ExpiryDateStart { get; set; }
        public DateTime? ExpiryDateEnd { get; set; }
        public DateTime? PaymentDateStart { get; set; }
        public DateTime? PaymentDateEnd { get; set; }
        public int? SalesPersonId { get; set; }
        public string? OrderStatus { get; set; }
        public string? SearchKeyword { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class FinanceOrderDetailDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int OrderItemId { get; set; }
        public int RowNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductSpecification { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal UnreceivedAmount { get; set; }
        public decimal PaymentRatio { get; set; }
        public DateTime? PaymentReceivedDate { get; set; }
        public int SalesPersonId { get; set; }
        public string SalesPersonName { get; set; } = string.Empty;
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FinanceOrderDetailListDto
    {
        public List<FinanceOrderDetailDto> Items { get; set; } = new List<FinanceOrderDetailDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public FinanceOrderSummaryDto Summary { get; set; } = new FinanceOrderSummaryDto();
    }

    public class FinanceOrderSummaryDto
    {
        public decimal TotalAmount { get; set; }
        public decimal TotalReceivedAmount { get; set; }
        public decimal TotalUnreceivedAmount { get; set; }
        public decimal TotalPaymentRatio { get; set; }
        public int OrderCount { get; set; }
        public int OrderItemCount { get; set; }
    }

    public class FinanceFilterOptionsDto
    {
        public List<FilterOptionDto> Customers { get; set; } = new List<FilterOptionDto>();
        public List<FilterOptionDto> Products { get; set; } = new List<FilterOptionDto>();
        public List<FilterOptionDto> SalesPersons { get; set; } = new List<FilterOptionDto>();
        public List<FilterOptionDto> OrderStatuses { get; set; } = new List<FilterOptionDto>();
        public List<FilterOptionDto> EffectiveDateOptions { get; set; } = new List<FilterOptionDto>();
        public List<FilterOptionDto> ExpiryDateOptions { get; set; } = new List<FilterOptionDto>();
        public List<FilterOptionDto> PaymentDateOptions { get; set; } = new List<FilterOptionDto>();
    }

    public class FilterOptionDto
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class UpdatePaymentInfoDto
    {
        public int OrderId { get; set; }
        public decimal ReceivedAmount { get; set; }
        public DateTime? PaymentReceivedDate { get; set; }
        public string? Remarks { get; set; }
    }

    public class FinanceOperationResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AffectedOrderCount { get; set; }
        public List<string> ErrorDetails { get; set; } = new List<string>();
    }
}