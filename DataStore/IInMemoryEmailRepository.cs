
using EIR_9209_2.Models;

public interface IInMemoryEmailRepository
{
    Task<Email> Add(Email email);
    Task<Email> Delete(string id);
    IEnumerable<Email> GetAll();
    Task<Email> Update(string id, Email email);
}