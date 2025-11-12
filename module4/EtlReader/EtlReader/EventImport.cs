namespace EtlReader
{
    public class EventImport
    {
        public Guid idLine;
        public string lineText;
        public int conta;
        public decimal value;
        public string descricao;
        public DateTime ProcessLine;
        public DateTime ProcessData;

        public DateTime? ErrorLine;
        public DateTime? ErrorData;
    }
}