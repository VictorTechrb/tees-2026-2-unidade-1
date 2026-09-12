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
  public async Task<Editora?> Get(int id)
  {
    return await _context.Editoras.FindAsync(id);
  }
  public async Task<IEnumerable<Editora>> GetAll()
  {
    return await _context.Editoras.AsNoTracking().ToListAsync();
  }
  public async Task<IEnumerable<Editora>> GetByNome(string nome)
{
	return await _context.Editoras
			.Where(e => e.Nome.StartsWith(nome))
			.OrderBy(e => e.Nome)
			.AsNoTracking()
			.ToListAsync();
}
  public async Task<IEnumerable<Editora>> GetByEstados()
{
	return await _context.Editoras
			.Where(e => e.Estado == "SP" || e.Estado == "RS")
			.AsNoTracking()
			.ToListAsync();
}
}