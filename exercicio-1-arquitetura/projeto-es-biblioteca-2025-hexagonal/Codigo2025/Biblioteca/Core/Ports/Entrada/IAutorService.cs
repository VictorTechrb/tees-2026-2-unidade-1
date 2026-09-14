using Core.Datatables;
using Core.DTO;

namespace Core.Ports.Entrada
{
    /// <summary>
    /// Porta de entrada (driving port) do agregado Autor.
    /// É por ela que os adaptadores de entrada — BibliotecaAPI e BibliotecaWeb — acionam
    /// o hexágono. As assinaturas são as mesmas da interface original em Core.Service:
    /// mudou o papel declarado, não o contrato.
    /// </summary>
    public interface IAutorService
    {
        uint Create(Autor autor);
        void Edit(Autor autor);
        void Delete(uint id);
        Autor? Get(uint id);
        IEnumerable<Autor> GetAll();
        IEnumerable<AutorDto> GetByNome(string nome);
        DatatableResponse<Autor> GetDataPage(DatatableRequest request);
    }
}
