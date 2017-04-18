using UnityEngine;
using System.Collections;

public class PerlinNoiseTerrainGenerator : MonoBehaviour {

    // Width of the terrain.
    protected int width = 128;
    // Height of the terrain.
    protected int height = 128;
    protected Color32[] colors;
    protected Texture2D texture;
    protected float[][] perlinNoise;
    protected float averageHeight;
    protected int planetValue;

    /// <summary>
    /// Method for unity object initialization.
    /// </summary>
    public void Start()
    {

        texture = new Texture2D(width, height);
        GetComponent<Renderer>().material.mainTexture = texture;
        colors = new Color32[width * height];


        perlinNoise = PerlinNoiseGenerator(WhiteNoiseGenerator(), 6);
        averageHeight = CalculateAverageHeight();
        Draw(perlinNoise);

        System.IO.StreamWriter file = new System.IO.StreamWriter("D:\\Education\\4course\\PCG\\speed.txt");
        file.WriteLine("Speed of Perlin Noise algorithm: " + (Time.realtimeSinceStartup * 1000));

        file.Close();

        texture.SetPixels32(colors);
        texture.Apply();
    }

    /// <summary>
    /// Update object on mouse click.
    /// </summary>
    public void Update()
    {

        // Click mouse button to generate new.
        if (Input.GetMouseButton(0))
        {
            perlinNoise = PerlinNoiseGenerator(WhiteNoiseGenerator(), 6);
            Draw(perlinNoise);
            texture.SetPixels32(colors);
            texture.Apply();
        }

    }

    /// <summary>
    /// Generates white noise.
    /// </summary>
    /// <returns>An array with random from 0 to 1 values.</returns>
    public float[][] WhiteNoiseGenerator()
    {
        float[][] whiteNoise = InitializeEmptyArray<float>(width, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                whiteNoise[i][j] = Random.Range(0f, 1f);
            }
        }

        return whiteNoise;
    }

    /// <summary>
    /// Creates smooth noise.
    /// </summary>
    /// <param name="basic">Array of the initial heights.</param>
    /// <param name="octave">Number of noizes to glue up together.</param>
    /// <returns>Array with smooth noise points.</returns>
    public float[][] SmoothNoiseGenerator(float[][] basic, int octave)
    {
        float[][] smoothNoise = InitializeEmptyArray<float>(width, height);

        int period = 1 << octave;
        float frequency = 1.0F / period;

        for (int i = 0; i < width; i++)
        {
            //horizontal sampling indices
            int sample0 = (i / period) * period;
            int sample1 = (sample0 + period) % width;
            float horizontalBlend = (i - sample0) * frequency;

            //vertical sampling indices
            for (int j = 0; j < height; j++)
            {
                int sample00 = (j / period) * period;
                int sample01 = (sample00 + period) % height;
                float verticalBlend = (j - sample00) * frequency;

                //blend the top two corners
                float top = Interpolate(basic[sample0][sample00],
                   basic[sample1][sample00], horizontalBlend);

                //blend the bottom two corners
                float bottom = Interpolate(basic[sample0][sample01],
                   basic[sample1][sample01], horizontalBlend);

                //final blend
                smoothNoise[i][j] = Interpolate(top, bottom, verticalBlend);

            }
        }


        return smoothNoise;
    }


    /// <summary>
    /// Linear Interpolation between two  points.
    /// </summary>
    /// <param name="x0">First point.</param>
    /// <param name="x1">Second point.</param>
    /// <param name="alpha">Blend on the axis.</param>
    /// <returns>Weighted average in between the two grid points.</returns>
    public static float Interpolate(float x0, float x1, float alpha)
    {
        return x0 * (1 - alpha) + alpha * x1;
    }

    /// <summary>
    /// Function to construct a heightmap of the terrain on the basis of
    /// Perlin noise algorithm.
    /// </summary>
    /// <param name="basic">Base heightmap of the terrain</param>
    /// <param name="octaveCount">Number of noises to glue together.</param>
    /// <returns>Heightmap for the terrain.</returns>
    public float[][] PerlinNoiseGenerator(float[][] basic, int octaveCount)
    {
        float[][][] smoothNoise = new float[octaveCount][][];

        float persistance = 0.5f;

        // Generate smooth noises.
        for (int i = 0; i < octaveCount; i++)
        {
            smoothNoise[i] = SmoothNoiseGenerator(basic, i);
        }

        float[][] perlinNoise = InitializeEmptyArray<float>(width, height);
        float amplitude = 1.0f;
        float totalAmplitude = 0.0f;

        // Glue noises together.
        for (int octave = octaveCount - 1; octave >= 0; octave--)
        {
            amplitude *= persistance;
            totalAmplitude += amplitude;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    perlinNoise[i][j] += smoothNoise[octave][i][j] * amplitude;
                }
            }
        }

        // Normalization of noise levels.
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                perlinNoise[i][j] /= amplitude;
            }
        }

        return perlinNoise;

    }

    /// <summary>
    /// Initialize an array with default values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="width">Number of columns in the two dimensional array.</param>
    /// <param name="height">Number of rows in the two dimensional array.</param>
    /// <returns>Array with default values.</returns>
    public static T[][] InitializeEmptyArray<T>(int width, int height)
    {
        T[][] array = new T[width][];

        for (int i = 0; i < width; i++)
        {
            array[i] = new T[height];
        }

        return array;
    }

    /// <summary>
    /// Calculates average height of the heightmap.
    /// </summary>
    /// <returns>Value of the average height of the map</returns>
    public float CalculateAverageHeight()
    {
        float totalHeight = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                totalHeight += perlinNoise[i][j];
            }
        }

        return (totalHeight / (height * width));
    }

    /// <summary>
    /// Assign a color to cocrete point on the base of it's height.
    /// Color sheme is used on the base of the planet type.
    /// </summary>
    /// <param name="pointHeight">Height of the terrain in the concrete point.</param>
    /// <returns>A color based on the height's value.</returns>
    public Color GetColor(float pointHeight)
    {

        if (planetValue == 1)
        {
            // WaterGrass coloring scheme.

            if (pointHeight < averageHeight)
            {
                return Color.blue;
            }
            else
            {
                return Color.green;
            }
        }
        else if (planetValue == 2)
        {
            // LavaAsh coloring scheme.

            if (pointHeight < averageHeight)
            {
                return new Color(0.647f, 0.1647f, 0.1647f);
            }
            else
            {
                return new Color(0.2F, 0.3F, 0.4F);
            }

        }
        else
        {
            // IceSnow coloring scheme.

            if (pointHeight < averageHeight)
            {
                return Color.gray;
            }
            else
            {
                return Color.white;
            }

        }
    }

    /// <summary>
    /// Forms a colors map of the terrain.
    /// </summary>
    /// <param name="heightMap">Array of heights in each point of the terrain.</param>
    public void Draw(float[][] perlinNoise)
    {
        int colorsNumber = 0;
        planetValue = Random.Range(1, 4);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                colors[colorsNumber] = GetColor(perlinNoise[i][j]);
                colorsNumber++;
            }
        }

    }
}
