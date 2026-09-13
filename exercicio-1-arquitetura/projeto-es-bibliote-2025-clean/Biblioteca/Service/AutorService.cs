using Core;
using Core.Datatables;
using Core.DTO;
using Core.Repository;
using Core.Service;

namespace Service;

/// <summary>
/// Implementa serviços para manter dados do autor
/// </summary>
public class AutorService : IAutorService
{
    private readonly IAutorRepository _autorRepository;

    public AutorService(IAutorRepository autorRepository)
    {
        _autorRepository = autorRepository;
    }

    /// <summary>Criar um novo autor — valida regra de negócio antes de persistir</summary>
    public uint Create(Autor autor)
    {
        if (autor.DataNascimento.Year < 1000)
            throw new ServiceException("O ano de nascimento de autor deve ser maior do que 1000. Favor informar nova data.");
        return _autorRepository.Create(autor);
    }

    /// <summary>Editar dados do autor — valida regra de negócio antes de persistir</summary>
    public void Edit(Autor autor)
    {
        if (autor.DataNascimento.Year < 1000)
            throw new ServiceException("O ano de nascimento de autor deve ser maior do que 1000. Favor informar nova data.");
        _autorRepository.Edit(autor);
    }

    public void Delete(uint id) => _autorRepository.Delete(id);
    public Autor? Get(uint id) => _autorRepository.Get(id).Result;
    public IEnumerable<Autor> GetAll() => _autorRepository.GetAll().Result;
    public IEnumerable<AutorDto> GetByNome(string nome) => _autorRepository.GetByNome(nome).Result;
    public DatatableResponse<Autor> GetDataPage(DatatableRequest request) => _autorRepository.GetDataPage(request).Result;
}
