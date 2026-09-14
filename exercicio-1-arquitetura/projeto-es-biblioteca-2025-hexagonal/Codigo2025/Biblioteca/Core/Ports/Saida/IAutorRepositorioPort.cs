using Core.Datatables;
using Core.DTO;

namespace Core.Ports.Saida
{
    /// <summary>
    /// Porta de saída (driven port) do agregado Autor.
    /// Declarada dentro do domínio e implementada fora dele: o hexágono define o que
    /// precisa da persistência, e os adaptadores (Entity Framework, memória) decidem como.
    /// Expõe apenas as operações que <see cref="Core.Service.IAutorService"/> oferece —
    /// a porta descreve a necessidade do domínio, não as capacidades do banco.
    /// </summary>
    public interface IAutorRepositorioPort
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
