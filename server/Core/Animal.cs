using System;
using System.Threading;
using System.Collections.Generic;
using server.Core;

namespace server
{

    public class Animal
    {

        public int x;

        public int y;

        public bool live = false;

        public int calorias = 100;

        public Semaphore main;

        public Semaphore semaphore = new Semaphore(0, 1);

        public Specie specie;

        private Thread thread;

        private Animal[,] grid;


        public static readonly Dictionary<Specie, List<Specie>> hierarquia = new Dictionary<Specie, List<Specie>>{
            { Specie.Shark, new List<Specie> { Specie.Fish, Specie.Seal } },
            { Specie.Seal, new List<Specie> { Specie.Fish } },
            { Specie.Fish, new List<Specie> { Specie.Seaweed } }
        };


        public Animal(Semaphore main, Specie specie, Animal[,] grid, int calorias)
        {
            this.calorias = calorias;
            this.main = main;
            this.specie = specie;
            this.grid = grid;

            var r = new Random();

            int i = 0;

            while (!live && i < 20)
            {
                this.x = r.Next(0, grid.GetLength(0));
                this.y = r.Next(0, grid.GetLength(1));
                if (this.grid[this.x, this.y] == null || !this.grid[x, y].live)
                {
                    this.grid[this.x, this.y] = this;
                    this.live = true;
                }
            }

            this.thread = new Thread(this.Run);
        }


        public void Start()
        {
            this.thread.Start();
        }

        public void Eat(Animal eat)
        {
            eat.live = false;
            this.calorias += eat.calorias;
        }

        public bool Verify(Animal a2)
        {
            if (this.specie != Specie.Seaweed && Animal.hierarquia[this.specie].Contains(a2.specie))
            {
                this.Eat(a2);
                return true;
            }
            else if (a2.specie != Specie.Seaweed && Animal.hierarquia[a2.specie].Contains(this.specie))
            {
                a2.Eat(this);
                return false;
            }
            else
            {
                return false;
            }
        }


        public void Walk()
        {
            var r = new Random();


            int nX = this.x;
            int nY = this.y;

            bool valid = false;

            while (!valid)
            {

                int pos = r.Next(4);

                switch (pos)
                {
                    case 0:
                        if (nY < this.grid.GetLength(1) - 1)
                        {
                            nY++;
                            valid = true;
                        }
                        break;

                    case 1:
                        if (nY > 0)
                        {
                            nY--;
                            valid = true;
                        }
                        break;

                    case 2:
                        if (nX < this.grid.GetLength(0) - 1)
                        {
                            nX++;
                            valid = true;
                        }
                        break;

                    case 3:
                        if (nX > 0)
                        {
                            nX--;
                            valid = true;
                        }
                        break;
                }

                if (valid)
                {
                    Animal a = this.grid[nX, nY];
                    if (a != null)
                    {
                        if (this.Verify(a))
                        {
                            this.Move(nX, nY);
                        }
                    }
                    else
                    {
                        this.Move(nX, nY);
                    }
                }
            }
        }

        public void Move(int nX, int nY)
        {
            grid[this.x, this.y] = null;

            this.x = nX;
            this.y = nY;
            grid[nX, nY] = this;

            this.calorias -= 5;

            if (this.calorias <= 0)
            {
                this.live = false;
            }
        }


        public void Run()
        {
            Console.WriteLine(this.specie.ToString() + " running");

            while (this.live)
            {
                if (this.semaphore.WaitOne())
                {
                    if (this.specie != Specie.Seaweed)
                    {
                        Console.WriteLine("Free a " + this.specie.ToString());

                        this.Walk();
                        Thread.Sleep(50);
                    }

                    this.main.Release(1);
                }
            }
        }

    }
}