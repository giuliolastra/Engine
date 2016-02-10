using System;

public class ResultCollector
{
    String VertexList;
    String[] AttributeList;
    int[] AttributeContent;

    int AttributeNumber;
    int definedAttributes;

    static int outputCounter = 0;

	public ResultCollector(int size)
    {
        this.AttributeNumber = size;
        AttributeList = new String[size];
        AttributeContent = new int[size];

        definedAttributes = 0;
	}

    public bool addNewAttribute(String AttributeName, int AttributeValue)
    {
        if(definedAttributes < AttributeNumber)
        {
            this.AttributeList[definedAttributes] = AttributeName;
            this.AttributeContent[definedAttributes] = AttributeValue;
            definedAttributes++;

            return true;
        }
        else
        {
            Console.WriteLine("Result Collector full");
            return false;
        }
    }

    public void addVertexName(String VertexName)
    {
        // Quando si fa l'output dei risultati bisogna rimuovere gli ultimi 2 caratteri
        this.VertexList = VertexName + ", " + this.VertexList; 
    }

    public void addAttribute(String AttributeName, int AttributeValue)
    {
        // Cerco il nome dell'attributo nella lista dei nomi, con l'indice vado a ritrovarlo nell'array dei valori e lo sommo.
        int position = Array.IndexOf(AttributeList, AttributeName);
        this.AttributeContent[position] = this.AttributeContent[position] + AttributeValue;
    }

    public Result FetchResult()
    {
        outputCounter++;
        return new Result(this.AttributeList[outputCounter - 1], this.AttributeContent[outputCounter - 1]);   
    } 

    public String getVertexList()
    { 
        return this.VertexList;
    }

    // per output 
    public String toString()
    {
        String Output = "Path: ";
        // Aggiungo alla stampa l'elenco dei vertici
        Output = Output + this.VertexList.Remove(this.VertexList.Length - 2) + Environment.NewLine + Environment.NewLine;

        //Itero e stampo tutti gli attributi già sommati
        for(int i = 0; i < this.definedAttributes; i++)
        {
            Output = Output + this.AttributeList[i] + ": " + this.AttributeContent[i].ToString() + Environment.NewLine;
        }
        return Output;
    }
}

public class Result
{
    String Name { get; }
    int Value { get; }

    public Result(String Name, int Value)
    {
        this.Name = Name;
        this.Value = Value;
    }
}