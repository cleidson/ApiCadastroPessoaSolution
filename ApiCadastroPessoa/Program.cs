using ApiCadastroPessoa.Models;
using Oracle.ManagedDataAccess.Client;

namespace ApiCadastroPessoa
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            var connectionString = builder.Configuration.GetConnectionString("OracleDb");

            // Endpoint para gerar 100 registros automaticamente
            app.MapPost("/pessoas/popular", async () =>
            {
                var pessoas = new List<Pessoa>();
                var random = new Random();
                var nomes = new[] { "João", "Maria", "Carlos", "Ana", "Bruno", "Lucas", "Paula", "Fabiana", "Pedro", "Fernanda" };

                using var conn = new OracleConnection(connectionString);
                await conn.OpenAsync();

                for (int i = 0; i < 100; i++)
                {
                    var nome = $"{nomes[random.Next(nomes.Length)]} {random.Next(1, 100)}";
                    var email = $"{nome.ToLower().Replace(" ", ".")}@teste.com";

                    var cmd = new OracleCommand("INSERT INTO CADASTRO.PESSOA (NOME, EMAIL) VALUES (:nome, :email)", conn);
                    cmd.Parameters.Add("nome", OracleDbType.Varchar2).Value = nome;
                    cmd.Parameters.Add("email", OracleDbType.Varchar2).Value = email;

                    await cmd.ExecuteNonQueryAsync();
                }

                return Results.Ok("100 pessoas foram inseridas com sucesso!");
            });



            // Endpoint GET /pessoas
            app.MapGet("/pessoas", async () =>
            {
                var pessoas = new List<Pessoa>();

                using var conn = new OracleConnection(connectionString);
                await conn.OpenAsync();

                using var cmd = new OracleCommand("SELECT ID, NOME, EMAIL FROM CADASTRO.PESSOA", conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    pessoas.Add(new Pessoa
                    {
                        Id = reader.GetInt32(0),
                        Nome = reader.GetString(1),
                        Email = reader.GetString(2)
                    });
                }

                return Results.Ok(pessoas);
            });

            // Endpoint POST /pessoas
            app.MapPost("/pessoas", async (Pessoa pessoa) =>
            {
                using var conn = new OracleConnection(connectionString);
                await conn.OpenAsync();

                using var cmd = new OracleCommand("INSERT INTO CADASTRO.PESSOA (NOME, EMAIL) VALUES (:nome, :email)", conn);
                cmd.Parameters.Add("nome", OracleDbType.Varchar2).Value = pessoa.Nome;
                cmd.Parameters.Add("email", OracleDbType.Varchar2).Value = pessoa.Email;

                await cmd.ExecuteNonQueryAsync();

                return Results.Created($"/pessoas/{pessoa.Email}", pessoa);
            });

            app.MapGet("/", () => "API Cadastro Pessoa está no ar");

            app.Run();
        }
    }
}
