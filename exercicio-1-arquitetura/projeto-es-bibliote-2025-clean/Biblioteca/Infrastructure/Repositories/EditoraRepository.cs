using Core;
using Core.Repository;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EditoraRepository : IEditoraRepository
{
  private readonly BibliotecaContext _context;
  public EditoraRepository(BibliotecaContext context)
  {
    _context = context;
  }
  public uint Create(Editora editora){
    _context.Add(editora);
    _context.SaveChanges();
    return editora.Id;    
  }
  public void Edit(Editora editora)
  {
    _context.Update(editora);
    _context.SaveChanges();
  }
  public void Delete(int id)
  {
    var editora = _context.Editoras.Find(id);
    if (editora != null)
    {
      _context.Remove(editora);
      _context.SaveChanges();
    }
  }
  public Editora? Get(int id)
  {
    return _context.Editoras.Find(id);
  }
  public IEnumerable<Editora> GetAll()
  {
    return _context.Editoras.AsNoTracking();
  }
  public IEnumerable<Editora> GetByNome(string nome)
  {
    return _context.Editoras
            .Where(e => e.Nome.StartsWith(nome)) 
            .OrderBy(e => e.Nome)                 
            .AsNoTracking();                       
  }
  public IEnumerable<Editora> GetByEstados()
  {
    return _context.Editoras.Where(e => e.Estado.Equals("SP") || e.Estado.Equals("RS")).AsNoTracking();
  }
}