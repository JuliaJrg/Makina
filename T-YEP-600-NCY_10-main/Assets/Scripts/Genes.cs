public class Genes
{
    public bool isMale;

    public static Genes RandomGenes(int seed)
    {
        System.Random random = new System.Random(seed);
        Genes genes = new Genes();
        genes.isMale = random.Next(0, 2) == 0;
        return genes;
    }
}