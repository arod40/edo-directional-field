using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectionalField
{
    public class SpringMassSystem
    {
        double[] mass;
        double[] stiffness;
        public double[,] quofMatrix;
        double[] x0;
        double[] v0;


        public SpringMassSystem(double[] mass, double[] stiffness, double[] x0, double[] v0)
        {
            int n = mass.Length;

            this.mass = mass;
            this.stiffness = stiffness;

            BuildQuofMatrix(mass.Length!=stiffness.Length);

            this.x0 = x0;
            this.v0 = v0;
        }
        public void BuildQuofMatrix(bool lastSpring)
        {
            int n = this.mass.Length;
            double[,] mass = MassInverse(this.mass);
            double[,] stiffness = Stiffness(this.stiffness, lastSpring);
            quofMatrix = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                quofMatrix[i, i] = stiffness[i, i] * mass[i, i];
                if (i != 0)
                    quofMatrix[i, i - 1] = stiffness[i, i - 1] * mass[i, i];
                if (i != n - 1)
                    quofMatrix[i, i + 1] = stiffness[i, i + 1] * mass[i, i];
            }

        }
        double[,] MassInverse(double[] mass)
        {
            int n = mass.Length;
            double[,] masses = new double[n, n];

            for (int i = 0; i < n; i++)
                masses[i, i] = 1 / mass[n - i - 1];

            return masses;
        }
        double[,] Stiffness(double[] stiff, bool lastSpring)
        {
            int n = 0;
            if (lastSpring)
                n = stiff.Length - 1;
            else
                n = stiff.Length;

            double[,] stiffness = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                stiffness[i, i] = -stiff[i];
                if (i != 0)
                    stiffness[i, i - 1] = stiff[i];
                if (i != n - 1)
                {
                    stiffness[i, i] -= stiff[i + 1];
                    stiffness[i, i + 1] = stiff[i + 1];
                }
                else if (lastSpring)//if there is a last spring then change the last term of the matrix
                    stiffness[i, i] = -(stiff[i] + stiff[i + 1]);
            }
            return stiffness;
        }

        //Euler's method for second order differential systems of linear equations
        public IEnumerable<double[]> EulerMethod(double h)
        {
            return EulerMethod(quofMatrix, x0, v0, h);
        }
        IEnumerable<double[]> EulerMethod(double[,] quofMatrix, double[] x0, double[] v0, double h)
        {
            //INITIALIZATIONS
            int n = x0.Length;
            double[] solutions = new double[2 * n + 1];
            double[] current = new double[2 * n + 1];
            solutions[0] = 0;
            for (int k = 1; k <= n; k++)
            {
                solutions[k] = x0[k - 1];
                solutions[k + n] = v0[k - 1];
            }
            yield return solutions;
            //ITERATIONS
            while (true)
            {
                current[0] = solutions[0] + h;//updating time
                for (int j = 1; j <= n; j++) //updating the displacement values
                    current[j] = solutions[j] + h * solutions[j + n];
                for (int j = n + 1; j <= 2 * n; j++) //updating the velocity values
                {
                    current[j] = 0;
                    for (int k = 1; k <= n; k++)
                        current[j] += solutions[k] * quofMatrix[j - n - 1, k - 1];
                    current[j] *= h;
                    current[j] += solutions[j];
                }
                yield return current;
                Array.Copy(current, solutions, current.Length);
            }
        }

    }
}
