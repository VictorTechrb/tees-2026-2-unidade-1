using Core;
using Core.DTO;
using Core.Repository;
using Core.Service;

namespace Service;

public class LivroService : ILivroService
{
    private readonly ILivroRepository _livroRepository;

    public LivroService(ILivroRepository livroRepository) 
    { 
        _livroRepository = livroRepository; 
    }

    public uint Create(Livro livro) => _livroRepository.Create(livro);
    public void Edit(Livro livro) => _livroRepository.Edit(livro);
    public void Delete(uint id) => _livroRepository.Delete(id);
    public Livro? Get(uint id) => _livroRepository.Get(id).Result;
    public IEnumerable<LivroDto> GetAll() => _livroRepository.GetAll();
    public IEnumerable<LivroDto> GetLivroDTO() => _livroRepository.GetLivroDTO();
    public IEnumerable<LivroDto> GetByNome(string nome) => _livroRepository.GetByNome(nome);
    public IEnumerable<Autor> GetAutoresByLivro(int idLivro) => _livroRepository.GetAutoresByLivro(idLivro);
    public IEnumerable<Livro> GetLivrosByNomeEditora(string nome) => _livroRepository.GetLivrosByNomeEditora(nome);
}
