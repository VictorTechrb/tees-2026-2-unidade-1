using Core;
using Core.Repository;
using Core.Service;

namespace Service;

public class EditoraService : IEditoraService
{
    private readonly IEditoraRepository _editoraRepository;

    public EditoraService(IEditoraRepository editoraRepository) 
    { 
        _editoraRepository = editoraRepository; 
    }

    public uint Create(Editora editora) => _editoraRepository.Create(editora);
    public void Edit(Editora editora) => _editoraRepository.Edit(editora);
    public void Delete(int id) => _editoraRepository.Delete(id);
    public Editora? Get(int id) => _editoraRepository.Get(id).Result;
    public IEnumerable<Editora> GetAll() => _editoraRepository.GetAll().Result;
    public IEnumerable<Editora> GetByNome(string nome) => _editoraRepository.GetByNome(nome).Result;
    public IEnumerable<Editora> GetByEstados() => _editoraRepository.GetByEstados().Result;
}
