using System;
using System.Threading;
using System.Collections.Generic;

namespace BehaviorTree
{
    // Enumeración de estados
    public enum Status { SUCCESS, FAILURE, RUNNING }

    // Nodo base
    public abstract class Node
    {
        protected List<Node> children = new List<Node>();
        public abstract Status Execute();
        public void AddChild(Node child) { children.Add(child); }
    }

    // Selector - Ejecuta hijos hasta que uno tenga éxito
    public class Selector : Node
    {
        public override Status Execute()
        {
            foreach (var child in children)
            {
                Status status = child.Execute();
                if (status != Status.FAILURE) return status;
            }
            return Status.FAILURE;
        }
    }

    // Secuencia - Ejecuta hijos en orden hasta que uno falle
    public class Sequence : Node
    {
        public override Status Execute()
        {
            foreach (var child in children)
            {
                Status status = child.Execute();
                if (status != Status.SUCCESS) return status;
            }
            return Status.SUCCESS;
        }
    }

    // Condición para verificar distancia
    public class DistanceCheck : Node
    {
        private Vector2 objetivo;
        public Vector2 posicionActual;
        private float distanciaValida;

        public DistanceCheck(Vector2 objetivo, Vector2 posicionActual, float distanciaValida)
        {
            this.objetivo = objetivo;
            this.posicionActual = posicionActual;
            this.distanciaValida = distanciaValida;
        }

        public override Status Execute()
        {
            return Vector2.Distancia(posicionActual, objetivo) <= distanciaValida ? Status.SUCCESS : Status.FAILURE;
        }
    }

    // Tarea para moverse hacia el objetivo
    public class MovimientoTarea : Node
    {
        private Vector2 objetivo;
        private float velocidad;
        private DistanceCheck verificadorDistancia;

        public MovimientoTarea(Vector2 objetivo, float velocidad, DistanceCheck verificadorDistancia)
        {
            this.objetivo = objetivo;
            this.velocidad = velocidad;
            this.verificadorDistancia = verificadorDistancia;
        }

        public override Status Execute()
        {
            Vector2 direccion = (objetivo - verificadorDistancia.posicionActual).Normalizar() * velocidad;
            verificadorDistancia.posicionActual += direccion;
            Console.WriteLine($"Moviendo a ({verificadorDistancia.posicionActual.x:F2}, {verificadorDistancia.posicionActual.y:F2})");

            return Vector2.Distancia(verificadorDistancia.posicionActual, objetivo) <= 0.1f ? Status.SUCCESS : Status.RUNNING;
        }
    }

    // Tarea para esperar un tiempo
    public class EsperaTarea : Node
    {
        private int tiempoEspera;
        public EsperaTarea(int tiempoEspera) { this.tiempoEspera = tiempoEspera; }
        public override Status Execute()
        {
            Console.WriteLine($"Esperando {tiempoEspera} ms...");
            Thread.Sleep(tiempoEspera);
            return Status.SUCCESS;
        }
    }

    // Clase para manejar vectores
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
        public static Vector2 operator *(Vector2 a, float b) => new Vector2(a.x * b, a.y * b);
        public Vector2 Normalizar() => Magnitud() > 0 ? new Vector2(x / Magnitud(), y / Magnitud()) : new Vector2(0, 0);
        public float Magnitud() => (float)Math.Sqrt(x * x + y * y);
        public static float Distancia(Vector2 a, Vector2 b) => (a - b).Magnitud();
    }

    // Programa principal
    public class Program
    {
        public static void Main(string[] args)
        {
            Vector2 objetivo = new Vector2(10, 10);
            Vector2 posicionInicial = new Vector2(0, 0);
            float distanciaValida = 1.0f;
            float velocidad = 0.5f;
            int tiempoEspera = 500;

            DistanceCheck verificadorDistancia = new DistanceCheck(objetivo, posicionInicial, distanciaValida);
            MovimientoTarea movimiento = new MovimientoTarea(objetivo, velocidad, verificadorDistancia);
            Selector selectorMovimiento = new Selector();
            selectorMovimiento.AddChild(verificadorDistancia);
            selectorMovimiento.AddChild(movimiento);

            EsperaTarea espera = new EsperaTarea(tiempoEspera);
            Sequence secuenciaPrincipal = new Sequence();
            secuenciaPrincipal.AddChild(selectorMovimiento);
            secuenciaPrincipal.AddChild(espera);

            Console.WriteLine("Iniciando IA...");
            Status estado;
            do
            {
                estado = secuenciaPrincipal.Execute();
            }
            while (estado != Status.SUCCESS);

            Console.WriteLine("Objetivo alcanzado.");
            Console.ReadKey();
        }
    }
}
