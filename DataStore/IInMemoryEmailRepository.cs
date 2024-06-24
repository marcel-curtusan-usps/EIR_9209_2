
using EIR_9209_2.Models;

public interface IInMemoryEmailRepository
{
    Email Add(Email email);
    Email Delete(string id);
    IEnumerable<Email> GetAll();
    Email Update(Email email);
}