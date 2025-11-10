using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Importador.Log
{
    [EventSource(Name = "Importador-Provider", Guid = "{5f506867-43a7-4eff-a2ca-a9a5212d64d4}")]
    public class ImportadorProvider : EventSource
    {
        public static ImportadorProvider Log = new ImportadorProvider();

        [Event(1, Level = EventLevel.Informational, Message = "O processo de importação foi iniciado")]
        public void StartedImport() => WriteEvent(1);

        [Event(2, Level = EventLevel.Informational, Message = "O processo de importação foi concluido")]
        public void CompletedImport() => WriteEvent(2);

        [Event(12, Level = EventLevel.Informational, Message = "Importando arquivo {0}")]
        public void ProcessFile(string fileName)
        {
            string json = JsonConvert.SerializeObject(new
            {
                fileName = fileName
            });

            byte[] bytes = Encoding.ASCII.GetBytes(json);

            WriteEvent(12, bytes);
        }

        [Event(13, Level = EventLevel.Informational, Message = "Processando linha {1}")]
        public void ProcessLine(string fileName, string line, string lineCode)
        {
            string json = JsonConvert.SerializeObject( new
            {
                fileName = fileName,
                line = line,
                lineCode = lineCode
            });

            byte[] bytes = Encoding.ASCII.GetBytes(json);

            WriteEvent(13, bytes);
        }

        [Event(14, Level = EventLevel.Informational, Message = "Processando dados da conta {2}, valor {3} - {4}")]
        public void ProcessData(string fileName, string lineCode, int conta, string valor, string descricao)
        {
            string json = JsonConvert.SerializeObject(new
            {
                fileName = fileName,
                lineCode = lineCode,
                conta = conta,
                valor = valor,
                descricao = descricao
            });

            byte[] bytes = Encoding.ASCII.GetBytes(json);

            WriteEvent(14, bytes);
        }

        [Event(21, Level = EventLevel.Error, Message = "Erro ao processar o Arquivo {0}")]
        public void ErrorProcess(string fileName)
        {
            string json = JsonConvert.SerializeObject(new
            {
                fileName = fileName
            });

            byte[] bytes = Encoding.ASCII.GetBytes(json);

            WriteEvent(21, bytes);
        }

        [Event(22, Level = EventLevel.Error, Message = "Erro ao processar a Linha {1}")]
        public void ErrorLine(string fileName, string line, string lineCode)
        {
            string json = JsonConvert.SerializeObject(new
            {
                fileName = fileName,
                line = line,
                lineCode = lineCode
            });

            byte[] bytes = Encoding.ASCII.GetBytes(json);

            WriteEvent(22, bytes);
        }

        [Event(23, Level = EventLevel.Error, Message = "Erro ao processar a Conta {2}")]
        public void ErrorProcessConta(string fileName, string lineCode, int conta)
        {
            string json = JsonConvert.SerializeObject(new
            {
                fileName = fileName,
                lineCode = lineCode,
                conta = conta
            });

            byte[] bytes = Encoding.ASCII.GetBytes(json);

            WriteEvent(23, bytes);
        }

    }
}
