using Core.Datatables;
using Core.DTO;
namespace Core.Repository;
public interface IAutorRepository
{
    uint Create(Autor autor);
    void Edit(Autor autor);
    void Delete(uint id);
    Autor? Get(uint id);
    IEnumerable<Autor> GetAll();
    IEnumerable<Autor> GetAllOrderByNome();
    int GetCountAutores();
    IEnumerable<Autor> GetByName(string nomeAutor);
    IEnumerable<Autor> GetOrderByDescending();
    IEnumerable<AutorDto> GetByNome(string nome);
    DatatableResponse<Autor> GetDataPage(DatatableRequest request);
}