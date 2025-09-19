/*
 * CLASSE DE PROFILING - PUC CAMPINAS
 * Este código será utilizado nas aulas de profiling de performance na PUC Campinas.
 * 
 * NOTA: Este código é fornecido apenas para fins de demonstração. Ele propositalmente contém problemas de performance para auxiliar na demonstração do profiling de performance.
 * 
 * Portions of this code are based on work by Mitchel Sellers
 * Original repository: https://github.com/mitchelsellers/memorydiagnosisexample
 * Licensed under MIT License
 * 
 * MIT License
 * Copyright (c) 2025 Mitchel Sellers
 */

using System.Text;

namespace PerformanceDemo;

internal class Program
{
    private const int StringIterations = 10000;

    private static void Main(string[] args)
    {
        Console.WriteLine("Bem-vindo à demonstração de performance");
        ListCommands();
        ClearFiles();

        var command = Console.ReadLine();
        while (command.ToLower() != "x")
        {
            switch (command.ToLower())
            {
                case "list":
                    ListCommands();
                    break;
                case "bad string":
                    BadStringManipulation();
                    break;
                case "good string":
                    GoodStringManipulation();
                    break;
                case "force collection":
                    ForceCollection();
                    break;
                case "good file":
                    GoodFileWritingExample();
                    break;
                case "bad file":
                    BadFileWritingExample();
                    break;
                case "clear file":
                    ClearFiles();
                    break;
                default:
                    Console.WriteLine("Comando desconhecido. Tente novamente. Digite 'list' para ver todos os comandos disponíveis.");
                    break;
            }

            Console.WriteLine("Por favor, digite seu próximo comando:");
            command = Console.ReadLine();
        }
    }

    /// <summary>
    ///     Exibe para o usuário uma lista dos comandos disponíveis
    /// </summary>
    private static void ListCommands()
    {
        Console.WriteLine(string.Empty);
        Console.WriteLine("Os seguintes comandos estão disponíveis:");
        Console.WriteLine("list = Mostra esta lista de ações");
        Console.WriteLine("-- Auxiliares de Coleta de Lixo --");
        Console.WriteLine("force collection = Força o coletor de lixo a executar");
        Console.WriteLine("-- Exemplos de Manipulação de String --");
        Console.WriteLine("bad string = Demonstra manipulação ruim de string com 10.000 iterações");
        Console.WriteLine("good string = Demonstra manipulação boa de string com 10.000 iterações");
        Console.WriteLine("-- Exemplos de Escrita de Arquivo --");
        Console.WriteLine("bad file = Demonstra escrita ruim de arquivo com 10.000 iterações");
        Console.WriteLine("good file = Demonstra escrita boa de arquivo com 10.000 iterações");
        Console.WriteLine(string.Empty);
        Console.WriteLine("Pressione X para sair");
    }

    #region Coleta de Lixo

    /// <summary>
    ///     Força o coletor de lixo a executar, durante a demonstração pode ser útil para redefinir/reduzir o uso de memória para
    ///     testes adicionais de linha de base/exemplo
    /// </summary>
    private static void ForceCollection()
    {
        GC.Collect();
    }

    #endregion

    #region Strings

    /// <summary>
    ///     Demonstra manipulação ruim de string usando o operador + para concatenar strings, o que resultará em maiores
    ///     alocações e strings que permanecerão na memória por mais tempo
    /// </summary>
    /// <remarks>
    ///     Embora este seja um exemplo extremo, considere as implicações reais de um site de alto tráfego com 10+ concatenações
    ///     de string por requisição. Isso pode se acumular rapidamente e causar problemas de performance.
    /// </remarks>
    private static void BadStringManipulation()
    {
        Console.WriteLine("Iniciando Manipulação Ruim de String");
        var myMessage = string.Empty;
        for (var i = 0; i < StringIterations; i++) myMessage += i.ToString();

        Console.WriteLine("Completadas 10.000 atualizações de string");
    }

    /// <summary>
    ///     Demonstra manipulação boa de string usando a classe StringBuilder para concatenar strings, o que resultará em
    ///     menos alocações e strings que permanecerão menos tempo na memória
    /// </summary>
    private static void GoodStringManipulation()
    {
        Console.WriteLine("Iniciando Manipulação Boa de String");
        var myMessage = new StringBuilder();
        for (var i = 0; i < StringIterations; i++) myMessage.Append(i);
        Console.WriteLine("Completadas 10.000 atualizações de string");
    }

    #endregion

    #region Arquivos

    /// <summary>
    ///     Escreve 10.000 arquivos no disco usando um exemplo ruim de escrita de arquivo, resultando em muitos handles de arquivo
    ///     sendo deixados abertos
    /// </summary>
    /// <remarks>
    ///     Além dos problemas de memória com o exemplo, a maioria desses arquivos também mostrará "Em uso por outro processo"
    ///     se tentarem ser modificados. Como tal, é importante notar que você pode não conseguir executar este
    ///     exemplo 2x seguidas
    /// </remarks>
    private static void BadFileWritingExample()
    {
        if (!Directory.Exists("badfile")) Directory.CreateDirectory("badfile");

        for (var i = 0; i < 10000; i++) WriteFileBad(i);
    }

    /// <summary>
    ///     Implementação interna da escrita de um único arquivo
    /// </summary>
    /// <param name="fileNumber"></param>
    private static void WriteFileBad(int fileNumber)
    {
        try
        {
            var fs = new FileStream($"badfile/{fileNumber}-example.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var writer = new StreamWriter(fs);
            writer.WriteLine("Eu sou um escritor de arquivo ruim");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }

    /// <summary>
    ///     Escreve 10.000 arquivos no disco usando um exemplo bom de escrita de arquivo, resultando em handles de arquivo gerenciados adequadamente
    /// </summary>
    private static void GoodFileWritingExample()
    {
        if (!Directory.Exists("goodfile")) Directory.CreateDirectory("goodfile");
        for (var i = 0; i < 10000; i++) WriteFileGood(i);
    }

    private static void WriteFileGood(int fileNumber)
    {
        using var fs = new FileStream($"goodfile/{fileNumber}example.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using var writer = new StreamWriter(fs);

        writer.WriteLine("Eu sou um escritor de arquivo bom!");
    }

    private static void ClearFiles()
    {
        if (Directory.Exists("goodfile")) Directory.Delete("goodfile", true);

        if (Directory.Exists("badfile")) Directory.Delete("badfile", true);
    }

    #endregion
}