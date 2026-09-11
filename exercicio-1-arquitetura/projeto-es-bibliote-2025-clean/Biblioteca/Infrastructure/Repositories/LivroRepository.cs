using Core;
using Core.Repository;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories;

public class LivroRepository : ILivroRepository
{
	private readonly BibliotecaContext _context;
	public LivroRepository(BibliotecaContext context)
	{
		_context = context;
	}

	public uint Create(Livro livro)
	{
		_context.Add(livro);
		_context.SaveChanges();
		return livro.Id;
	}

	public void Delete(uint id)
	{
		var livro = context.Livros.Find(id);
		if (livro != null)
		{
			_context.Remove(livro);
			_context.SaveChanges();
		}
	}

	public void Edit(Livro livro)
	{
		_context.Update(livro);
		_context.SaveChanges();
	}

	public Livro? Get(uint id)	{
		return  _context.Livros
			.Include(l = &gt; l.Editora)
			.FirstOrDefaultAsync(l =&gt; l.Id == id);
	}

	public IEnumerable<LivroDto> GetLivroDTO()
	{
		var query = from livro in context.Livros
					select new LivroDto
					{
						Id = livro.Id,
						Isbn = livro.Isbn,
						Nome = livro.Nome,
						NomeEditora = livro.IdEditoraNavigation.Nome
					};
		return query.AsNoTracking();
	}

	public IEnumerable<LivroDto> GetAll()
	{
		var query = from livro in _context.Livros
					orderby livro.Nome descending
					select new LivroDto
					{
						Id = livro.Id,
						Nome = livro.Nome,
						Isbn = livro.Isbn,
						NomeEditora = livro.IdEditoraNavigation.Nome
					};
		return query;
	}

	public IEnumerable<Autor> GetAutoresByLivro(int idLivro)
	{
		var livro =  _context.Livros
			.Include(l = &gt; l.IdAutors) 
			.FirstOrDefaultAsync(l = &gt; l.Id == idLivro); 
		return livro?.IdAutors ?? new List
	}

	public IEnumerable<Livro> GetLivrosByNomeEditora(string nome)
	{
		return _context.Livros
			.Include(l = &gt; l.IdEditoraNavigation) 
		.Where(l = &gt; l.IdEditoraNavigation.Nome.StartsWith(nome)) 
		.ToListAsync();
	}
	public IEnumerable<LivroDto> GetByNome(string nome)
	{
		var query = from livro in context.Livros
					where livro.Nome.StartsWith(nome)
					orderby livro.Nome
					select new LivroDto
					{
						Id = livro.Id,
						Nome = livro.Nome,
						Isbn = livro.Isbn,
						NomeEditora = livro.IdEditoraNavigation.Nome
					};
		return query;
	}
}


