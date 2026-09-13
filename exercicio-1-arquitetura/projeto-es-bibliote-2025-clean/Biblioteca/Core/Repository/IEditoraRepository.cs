namespace Core.Repository;

public interface IEditoraRepository
{
    uint Create(Editora editora);
    void Edit(Editora editora);
    void Delete(int id);
    Task<Editora?> Get(int id);
    Task<IEnumerable<Editora>> GetAll();
    Task<IEnumerable<Editora>> GetByNome(string nome);
    Task<IEnumerable<Editora>> GetByEstados();
}
