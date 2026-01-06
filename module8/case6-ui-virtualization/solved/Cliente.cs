namespace UIVirtualization.Solved;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Id} - {Nome} ({Cidade}/{Estado})";
    }
}
