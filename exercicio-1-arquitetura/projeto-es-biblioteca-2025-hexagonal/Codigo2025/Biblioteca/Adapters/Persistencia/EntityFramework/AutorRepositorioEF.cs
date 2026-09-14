using Core;
using Core.Datatables;
using Core.DTO;
using Core.Ports.Saida;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Persistencia.EntityFramework
{
    /// <summary>
    /// Adaptador de saída (driven adapter) do agregado Autor sobre Entity Framework.
    /// Implementa a porta <see cref="IAutorRepositorioPort"/> declarada no domínio:
    /// aqui mora apenas o "como" persistir — as regras de negócio ficam no hexágono.
    /// </summary>
    public class AutorRepositorioEF : IAutorRepositorioPort
    {
        private readonly BibliotecaContext context;

        public AutorRepositorioEF(BibliotecaContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Criar um novo autor na base de dados
        /// </summary>
        /// <param name="autor">dados do autor</param>
        /// <returns>id do autor</returns>
        public uint Create(Autor autor)
        {
            context.Add(autor);
            context.SaveChanges();
            return autor.Id;
        }

        /// <summary>
        /// Editar dados do autor na base de dados
        /// </summary>
        /// <param name="autor">dados do autor</param>
        public void Edit(Autor autor)
        {
            context.Update(autor);
            context.SaveChanges();
        }

        /// <summary>
        /// Remover o autor da base de dados
        /// </summary>
        /// <param name="id">id do autor</param>
        public void Delete(uint id)
        {
            var autor = context.Autors.Find(id);
            if (autor != null)
            {
                context.Remove(autor);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Buscar um autor na base de dados
        /// </summary>
        /// <param name="id">id do autor</param>
        /// <returns>dados do autor</returns>
        public Autor? Get(uint id)
        {
            return context.Autors.Find(id);
        }

        /// <summary>
        /// Buscar todos os autores cadastrados
        /// </summary>
        /// <returns>lista de autores</returns>
        public IEnumerable<Autor> GetAll()
        {
            return context.Autors.AsNoTracking();
        }

        /// <summary>
        /// Buscar autores iniciando com o nome
        /// </summary>
        /// <param name="nome">nome do autor</param>
        /// <returns>lista de autores que inicia com o nome</returns>
        public IEnumerable<AutorDto> GetByNome(string nome)
        {
            var query = from autor in context.Autors
                        where autor.Nome.StartsWith(nome)
                        orderby autor.Nome
                        select new AutorDto
                        {
                            Id = autor.Id,
                            Nome = autor.Nome
                        };
            return query;
        }

        /// <summary>
        /// Retorna uma página de dados
        /// </summary>
        /// <param name="request">parâmetros de paginação, busca e ordenação</param>
        /// <returns>página de autores</returns>
        public DatatableResponse<Autor> GetDataPage(DatatableRequest request)
        {
            var autores = context.Autors.AsNoTracking();
            // total de registros na tabela
            var totalRecords = autores.Count();

            // filtra pelo campos de busca
            if (request.Search != null && request.Search.GetValueOrDefault("value") != null)
            {
                var searchValue = request.Search.GetValueOrDefault("value") ?? string.Empty;
                autores = autores.Where(autor => autor.Id.ToString().Contains(searchValue)
                                              || autor.Nome.ToLower().Contains(searchValue));
            }

            // ordenação pelas colunas permitidas
            if (request.Order != null && request.Order[0].GetValueOrDefault("column").Equals("0"))
            {
                if (request.Order[0].GetValueOrDefault("dir").Equals("asc"))
                    autores = autores.OrderBy(autor => autor.Id);
                else
                    autores = autores.OrderByDescending(autor => autor.Id);
            }
            else if (request.Order != null && request.Order[0].GetValueOrDefault("column").Equals("1"))
            {
                if (request.Order[0].GetValueOrDefault("dir").Equals("asc"))
                    autores = autores.OrderBy(autor => autor.Nome);
                else
                    autores = autores.OrderByDescending(autor => autor.Nome);
            }
            else if (request.Order != null && request.Order[0].GetValueOrDefault("column").Equals("2"))
            {
                if (request.Order[0].GetValueOrDefault("dir").Equals("asc"))
                    autores = autores.OrderBy(autor => autor.DataNascimento);
                else
                    autores = autores.OrderByDescending(autor => autor.DataNascimento);
            }

            // total de registros filtrados
            int countRecordsFiltered = autores.Count();
            // paginação que será exibida
            autores = autores.Skip(request.Start).Take(request.Length);
            return new DatatableResponse<Autor>()
            {
                Data = autores.ToList(),
                Draw = request.Draw,
                RecordsFiltered = countRecordsFiltered,
                RecordsTotal = totalRecords
            };
        }
    }
}
