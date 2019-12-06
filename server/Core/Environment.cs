using System;
using System.Threading;
using System.Collections.Generic;
using server.Models;

namespace server.Core
{
    public class Environment
    {

        public Animal[,] grid;
        public Semaphore main = new Semaphore(1, 1);
        public List<Animal> animals = new List<Animal>();

        private Thread thread;

        public Environment(int width, int height)
        {
            int maxX = (width) / 100;
            int maxY = (height) / 50;

            this.grid = new Animal[maxX, maxY];

            this.thread = new Thread(Run);
        }

        public void AddAnimal(Specie specie, int calorias)
        {
            var animal = new Animal(this.main, specie, this.grid, calorias);
            this.animals.Add(animal);
            animal.Start();
        }

        public int GetLives()
        {
            int cont = 0;
            foreach (var animal in this.animals)
            {
                if (animal.live)
                {
                    cont++;
                }
            }
            return cont;
        }


        public AnimalModel[] GetGrid()
        {

            var total = this.GetLives();
            AnimalModel[] animals = new AnimalModel[total];

            int i = 0;
            for (int x = 0; x < this.grid.GetLength(0); x += 1)
            {
                for (int y = 0; y < this.grid.GetLength(1); y += 1)
                {

                    var animal = this.grid[x, y];

                    if (animal != null && animal.live)
                    {
                        animals[i++] = new AnimalModel
                        {
                            specie = animal.specie,
                            x = x * 100,
                            y = y * 50,
                            calorias = animal.calorias
                        };
                    }
                }
            }

            return animals;
        }

        public void Start()
        {
            this.thread.Start();
        }


        public void Run()
        {
            Random r = new Random();

            while (true)
            {
                if (this.main.WaitOne())
                {

                    bool live = false;
                    while (!live)
                    {
                        if (this.animals.Count > 0)
                        {
                            int index = r.Next(animals.Count);
                            Animal a = animals[index];

                            if (a != null && a.live)
                            {
                                a.semaphore.Release(1);
                                live = true;
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }

                }
            }
        }

        public void Destroy()
        {
            foreach (Animal animal in this.animals)
            {
                animal.live = false;
            }
        }

    }
}