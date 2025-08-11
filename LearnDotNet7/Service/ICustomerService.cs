using LearnDotNet7.helper;
using LearnDotNet7.Modal;
using LearnDotNet7.Repos.Models;

namespace LearnDotNet7.Service
{
    public interface ICustomerService
    {
        Task<List<Customermodal>> Getall();
        Task<Customermodal> Getbycode(string code);
        Task<APIResponse> Remove(string code);
        Task<APIResponse> Create(Customermodal data);

        Task<APIResponse> Update(Customermodal data, string code);
    }
}
