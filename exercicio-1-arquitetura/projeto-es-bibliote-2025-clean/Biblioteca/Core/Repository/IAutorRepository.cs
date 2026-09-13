using Core.Datatables;
using Core.DTO;
namespace Core.Repository;
public interface IAutorRepository
{
    uint Create(Autor autor);
    void Edit(Autor autor);
    void Delete(uint id);
    Task<Autor?> Get(uint id);
    Task<IEnumerable<Autor>> GetAll();
    Task<IEnumerable<Autor>> GetAllOrderByNome();
    Task<int> GetCountAutores();
    Task<IEnumerable<Autor>> GetByName(string nomeAutor);
    Task<IEnumerable<Autor>> GetOrderByDescending();
    Task<IEnumerable<AutorDto>> GetByNome(string nome);
    Task<DatatableResponse<Autor>> GetDataPage(DatatableRequest request);
}