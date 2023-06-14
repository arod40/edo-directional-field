using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectionalField
{

    public class Expresion {
        public string representation;
        public Expresion(string representacion)
        {           
            this.representation = representacion;
        }

        public Expresion()
        {
        }
         public virtual double Evaluar(Dictionary<char,double> asignaciones)
        {
            throw new Exception("Expresion?");
        }
    }
    class ExpresionCompuesta:Expresion
    {
        public Operator operador;
        public Expresion operando1;
        public Expresion operando2;
        public ExpresionCompuesta(string string0,List<Operator> operadores):base(string0)
        {
            
            //Acomodo
            SortedList<int,Operator> operadoresinvolucrados = new SortedList<int, Operator>() ;
            for (int i = 0; i < operadores.Count; i++)
            {
                Contiene(string0, operadores[i], operadoresinvolucrados);
            }
            SortedList<int,Expresion> miembros = Splitear(operadoresinvolucrados, string0);

            List<object> terminos = Merge(operadoresinvolucrados,miembros);
            
            //Aki comienza el algoritmo 
            Stack<Operator> operadore=new Stack<Operator>();
            Stack<Expresion> operandos = new Stack<Expresion>();

            ExpresionCompuesta x=null;
            for (int i = 0; i < terminos.Count; i++)
            {
                if (terminos[i] is Constant)
                {
                    operandos.Push(terminos[i] as Constant);
                }
                else if (terminos[i] is Variable)
                {
                    operandos.Push(terminos[i] as Variable); 
                }
                else
                {
                    Operator op = (Operator)terminos[i];
                    if (op.representation == "(") { operadore.Push(op); }
                    else if (op.representation == ")")
                    {
                        bool b = false;
                        while (operadore.Count > 0)
                        {
                            if (operadore.Peek().representation == "(")
                            { operadore.Pop(); b = true; break; }

                            if (operadore.Peek().cantOperadores == 2)
                            {
                                Expresion operando2 = operandos.Pop();
                                Operator operador = operadore.Pop();
                                Expresion operando1 = operandos.Pop();
                                string representation = operando1.representation + operador.representation + operando2.representation;
                                x = new ExpresionCompuesta();
                                x.operador = operador;
                                x.operando1 = operando1;
                                x.operando2 = operando2;
                                x.representation = representation;
                                operandos.Push(x);
                            }
                            else if (operadore.Peek().cantOperadores == 1)
                            {
                                Expresion operando2 = operandos.Pop();
                                Operator operador = operadore.Pop();
                                string representation = operador.representation + operando2.representation;
                                x = new ExpresionCompuesta();
                                x.operador = operador;
                                x.operando2 = operando2;
                                x.representation = representation;
                                operandos.Push(x);
                            }
                        }
                        if (b == false) { throw new Exception("Error de Sintaxis"); }
                        operandos.Peek().representation = "(" + operandos.Peek().representation + ")";
                    }
                    else
                    {
                        while (operadore.Count > 0 && operadore.Peek().prioridad >= op.prioridad)
                        {
                            if (operadore.Peek().cantOperadores == 2)
                            {
                                Expresion operando2 = operandos.Pop();
                                Operator operador = operadore.Pop();
                                Expresion operando1 = operandos.Pop();
                                string representation = operando1.representation + operador.representation + operando2.representation;
                                x = new ExpresionCompuesta();
                                x.operador = operador;
                                x.operando1 = operando1;
                                x.operando2 = operando2;
                                x.representation = representation;
                                operandos.Push(x);
                            }
                            else if(operadore.Peek().cantOperadores == 1)
                            {
                                Expresion operando2 = operandos.Pop();
                                Operator operador = operadore.Pop();
                                string representation = operador.representation + operando2.representation;
                                x = new ExpresionCompuesta();
                                x.operador = operador;
                                x.operando2 = operando2;
                                x.representation = representation;
                                operandos.Push(x);
                            }
                        }
                        operadore.Push(op);
                    }
                }    
            }

            while (operadore.Count > 0)
            {
                if (operadore.Peek().cantOperadores == 2)
                {
                    Expresion operando2 = operandos.Pop();
                    Operator operador = operadore.Pop();
                    Expresion operando1 = operandos.Pop();
                    string representation = operando1.representation + operador.representation + operando2.representation;
                    x = new ExpresionCompuesta();
                    x.operador = operador;
                    x.operando1 = operando1;
                    x.operando2 = operando2;
                    x.representation = representation;
                    operandos.Push(x);
                }
                else if (operadore.Peek().cantOperadores == 1)
                {
                    Expresion operando2 = operandos.Pop();
                    Operator operador = operadore.Pop();
                    string representation = operador.representation + operando2.representation;
                    x = new ExpresionCompuesta();
                    x.operador = operador;
                    x.operando2 = operando2;
                    x.representation = representation;
                    operandos.Push(x);
                }
            }

            x = (ExpresionCompuesta)operandos.Pop();
            if (operandos.Count > 0) { throw new Exception("Error de Sintaxis"); }
            this.operador = x.operador;
            this.operando1 = x.operando1;
            this.operando2 = x.operando2;
            
        }

        private void Contiene(string string0, Operator operat, SortedList<int, Operator> op)
        {
            string r= operat.representation;
            for (int i = 0; i <string0.Length; i++)
            {
                bool b = false;
                for (int j = 0; j < r.Length;j++)
                {
                    if (string0[i+j] == r[j]) { b = true; }
                    else { b = false; break; }
                }
                if (b)
                {
                    op.Add(i, operat);
                }
            }

        }

        private List<object> Merge(SortedList<int,Operator> operadoresinvolucrados, SortedList<int, Expresion> miembros)
        {
            List<object> result=new List<object>();
            int contOp = 0;
            int contmiem = 0;
            while (operadoresinvolucrados.Count-contOp > 0 || miembros.Count- contmiem > 0)
            {
                if (miembros.Count - contmiem == 0)
                {
                    for (int i = contOp; i < operadoresinvolucrados.Count; i++)
                    {
                        result.Add(operadoresinvolucrados.ElementAt(i).Value);
                        contOp++;
                    }
                }
                else if (operadoresinvolucrados.Count - contOp == 0)
                {
                    for (int i = contmiem; i < miembros.Count; i++)
                    {
                        result.Add(miembros.ElementAt(i).Value);
                        contmiem++;
                    }
                }
                else
                {
                    if (operadoresinvolucrados.ElementAt(contOp).Key < miembros.ElementAt(contmiem).Key)
                    {
                        result.Add(operadoresinvolucrados.ElementAt(contOp).Value);
                        contOp++;
                    }
                    else
                    {
                        result.Add(miembros.ElementAt(contmiem).Value);
                        contmiem++;
                    }
                }
            }

            return result;
        }

        public ExpresionCompuesta()
        {
        }

        private bool EsVariable(string v)
        {
            return v.Length == 1 && v != "1" && v != "2" && v != "3" && v != "4" && v != "5" && v != "6" && v != "7" && v != "8" && v != "9" && v != "0";
        }

        private SortedList<int,Expresion> Splitear(SortedList<int,Operator> operadoresinvolucrados, string string0)
        {

            SortedList<int,Expresion> result = new SortedList<int, Expresion>();
            double d;
            string r;
            int startindex = 0;
            foreach (var item in operadoresinvolucrados)
            {
                    r = string0.Substring(startindex, item.Key - startindex);
                    if (r.Length > 0)
                    {
                        if (double.TryParse(r, out d))
                        { result.Add(startindex,new Constant(r)); }
                        else if (EsVariable(r))
                        { result.Add(startindex,new Variable(r)); }
                        else
                        {
                            throw new Exception("Variable de mas de un caracter");
                        }
                    }
                    startindex = item.Key+item.Value.representation.Length;
                
            }
            //el ultimo token 
            r = string0.Substring(startindex, string0.Length - startindex);
            if (r.Length > 0)
            {
                if (double.TryParse(r, out d))
                { result.Add(startindex, new Constant(r)); }
                else if (EsVariable(r))
                { result.Add(startindex, new Variable(r)); }
                else
                {
                    throw new Exception("Variable de mas de un caracter");
                }
            }

            return result;

        }

        public override double Evaluar(Dictionary<char, double> asignaciones)
        {
            double d1=0;
            double d2=0;
            if (operando1 is ExpresionCompuesta){ d1=((ExpresionCompuesta)operando1).Evaluar(asignaciones); }
            else if (operando1 is Variable) { d1=((Variable)operando1).Evaluar(asignaciones); }
            else if (operando1 is Constant) { d1=((Constant)operando1).Evaluar(asignaciones); }

            if (operando2 is ExpresionCompuesta) { d2=((ExpresionCompuesta)operando2).Evaluar(asignaciones); }
            else if (operando2 is Variable) { d2=((Variable)operando2).Evaluar(asignaciones); }
            else if (operando2 is Constant) { d2=((Constant)operando2).Evaluar(asignaciones); }

            return operador.operar(d1, d2);

        }
    }

    delegate double Operar(double val1, double val2);

    class Operator
    {
        public int prioridad;
        public string representation;
        public Operar operar;
        public int cantOperadores;       
        public Operator(string representation,int prioridad,int cantOperadores,Operar operar)
        {
            this.representation = representation;
            this.prioridad = prioridad;
            this.operar = operar;
            this.cantOperadores = cantOperadores;
        }

    }
    class Variable : Expresion
    {
        char simbolo;
        
        public Variable(string string0):base(string0)
        {
            this.simbolo=string0[0];
        }

        public override double Evaluar(Dictionary<char, double> asignaciones)
        {
            if (!asignaciones.ContainsKey(simbolo))
                throw new Exception("Variable invalida.");
                return asignaciones[simbolo];
        }
    }
    class Constant : Expresion
    {
        double value;

        public Constant(string string0) : base(string0)
        {
          value = Double.Parse(string0);
        }

        public override double Evaluar(Dictionary<char, double> asignaciones)
        {
            return value;
        }
    }
}
