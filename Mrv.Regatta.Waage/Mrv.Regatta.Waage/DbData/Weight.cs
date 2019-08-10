namespace Mrv.Regatta.Waage.DbData
{
    public class Weight
    {
        public float? Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Weight"/> class.
        /// </summary>
        public Weight()
        {
            Value = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Weight" /> class.
        /// </summary>
        /// <param name="weightInKiloGram">The weight in kilo gram.</param>
        public Weight(float? weightInKiloGram)
        {
            Value = weightInKiloGram;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Weight" /> class.
        /// </summary>
        /// <param name="weightInGram">The weight in gram.</param>
        /// <param name="convert0ToNull">if set to <c>true</c> [convert0 to null].</param>
        public Weight(int? weightInGram, bool convert0ToNull)
        {
            if (weightInGram == null)
            {
                Value = null;
                return;
            }

            if (convert0ToNull)
            {
                if (weightInGram == 0)
                {
                    Value = null;
                    return;
                }
            }

            Value = (float)weightInGram / 1000;
        }

        /// <summary>
        /// Gibt das Gewicht im Format "xx kg" aus bzw. als "-" wenn nicht angegeben
        /// </summary>
        /// <returns></returns>
        public string ToDisplayString()
        {
            if (Value == null)
            {
                return "-";
            }

            return $"{Value} kg";
        }

        /// <summary>
        /// Determines whether the weight is specified.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the weight is specified; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSpecified()
        {
            return (Value != null);
        }
    }
}
