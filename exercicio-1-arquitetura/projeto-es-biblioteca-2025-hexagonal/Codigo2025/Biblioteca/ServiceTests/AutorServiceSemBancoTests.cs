using Application.Tests.Fakes;
using Core;
using Core.Ports.Entrada;
using Core.Ports.Saida;
using Core.Service;

// Nenhum using de Entity Framework, nenhum DbContext, nenhum banco em memória do EF.
// Se este arquivo compila e os testes passam, a regra de negócio do agregado Autor
// está isolada da persistência.

namespace Application.Tests
{
    /// <summary>
    /// Exercita o serviço do hexágono ligado ao adaptador em memória.
    /// Prova de isolamento: a mesma porta de saída que recebe o Entity Framework
    /// em produção recebe aqui uma lista simples.
    /// </summary>
    [TestClass()]
    public class AutorServiceSemBancoTests
    {
        private IAutorRepositorioPort repositorio;
        private IAutorService autorService;

        [TestInitialize]
        public void Initialize()
        {
            //Arrange
            repositorio = new AutorRepositorioEmMemoria();

            var autores = new List<Autor>
                {
                    new() { Id = 1, Nome = "Machado de Assis", DataNascimento =  DateTime.Parse("1917-12-31")},
                    new() { Id = 2, Nome = "Ian S. Sommervile", DataNascimento = DateTime.Parse("1935-12-31")},
                    new() { Id = 3, Nome = "Gleford Myers", DataNascimento = DateTime.Parse("1900-11-20")},
                };

            foreach (var autor in autores)
                repositorio.Create(autor);

            autorService = new AutorService(repositorio);
        }

        [TestMethod()]
        public void CreateTest()
        {
            // Act
            autorService.Create(new Autor() { Id = 4, Nome = "Graciliano Ramos", DataNascimento = DateTime.Parse("1900-12-25") });
            // Assert
            Assert.AreEqual(4, autorService.GetAll().Count());
            var autor = autorService.Get(4);
            Assert.AreEqual("Graciliano Ramos", autor.Nome);
            Assert.AreEqual(DateTime.Parse("1900-12-25"), autor.DataNascimento);
        }

        [TestMethod()]
        public void DeleteTest()
        {
            // Act
            autorService.Delete(2);
            // Assert
            Assert.AreEqual(2, autorService.GetAll().Count());
            var autor = autorService.Get(2);
            Assert.AreEqual(null, autor);
        }

        [TestMethod()]
        public void EditTest()
        {
            //Act
            var autor = autorService.Get(3);
            autor.Nome = "Paulo Coelho";
            autor.DataNascimento = DateTime.Parse("1950-11-21");
            autorService.Edit(autor);
            //Assert
            autor = autorService.Get(3);
            Assert.IsNotNull(autor);
            Assert.AreEqual("Paulo Coelho", autor.Nome);
            Assert.AreEqual(DateTime.Parse("1950-11-21"), autor.DataNascimento);
        }

        [TestMethod()]
        public void GetTest()
        {
            var autor = autorService.Get(1);
            Assert.IsNotNull(autor);
            Assert.AreEqual("Machado de Assis", autor.Nome);
            Assert.AreEqual(DateTime.Parse("1917-12-31"), autor.DataNascimento);
        }

        [TestMethod()]
        public void GetAllTest()
        {
            // Act
            var listaAutor = autorService.GetAll();
            // Assert
            Assert.IsInstanceOfType(listaAutor, typeof(IEnumerable<Autor>));
            Assert.IsNotNull(listaAutor);
            Assert.AreEqual(3, listaAutor.Count());
            Assert.AreEqual((uint)1, listaAutor.First().Id);
            Assert.AreEqual("Machado de Assis", listaAutor.First().Nome);
        }

        [TestMethod()]
        public void GetByNomeTest()
        {
            //Act
            var autores = autorService.GetByNome("Machado");
            //Assert
            Assert.IsNotNull(autores);
            Assert.AreEqual(1, autores.Count());
            Assert.AreEqual("Machado de Assis", autores.First().Nome);
        }

        // ------------------------------------------------------------------
        // Regra de negócio do agregado Autor.
        // Não havia nenhum teste para ela no projeto original, porque exercitá-la
        // exigia levantar um DbContext. Com a porta de saída, virou trivial.
        // ------------------------------------------------------------------

        [TestMethod()]
        public void CreateComAnoDeNascimentoAnteriorA1000LancaServiceException()
        {
            // Arrange
            var autor = new Autor() { Id = 5, Nome = "Autor Invalido", DataNascimento = new DateTime(999, 1, 1) };
            // Act + Assert
            Assert.ThrowsException<ServiceException>(() => autorService.Create(autor));
            // o autor nao pode ter sido persistido
            Assert.AreEqual(3, autorService.GetAll().Count());
        }

        [TestMethod()]
        public void EditComAnoDeNascimentoAnteriorA1000LancaServiceException()
        {
            // Arrange
            var autor = autorService.Get(1);
            Assert.IsNotNull(autor);
            autor.DataNascimento = new DateTime(999, 1, 1);
            // Act + Assert
            Assert.ThrowsException<ServiceException>(() => autorService.Edit(autor));
        }
    }
}
