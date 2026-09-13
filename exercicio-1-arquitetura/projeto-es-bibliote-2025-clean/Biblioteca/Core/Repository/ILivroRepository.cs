using Core.DTO;
namespace Core.Repository;
public interface ILivroRepository
{
    uint Create(Livro livro);
    void Edit(Livro livro);
    void Delete(uint id);
    Task<Livro?> Get(uint id);
    IEnumerable<LivroDto> GetAll();
    IEnumerable<LivroDto> GetLivroDTO();
    IEnumerable<LivroDto> GetByNome(string nome);
    IEnumerable<Autor> GetAutoresByLivro(int idLivro);
    IEnumerable<Livro> GetLivrosByNomeEditora(string nome);
}
