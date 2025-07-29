namespace HerramientasdeProgramacion.API.Services
{
    public class PlanServices
    {
        public static double GetPrecioMensual(string plan)
        {
            return plan.ToLower() switch
            {
                "free" => 0.0,
                "personal" => 1.0,
                "familiar" => 3.5,
                "empresarial" => 35.0,
                _ => 0.0
            };
        }

        public static int GetMaxDispositivos(string plan)
        {
            return plan.ToLower() switch
            {
                "free" => 1,
                "personal" => 1,
                "familiar" => 4,
                "empresarial" => 50,
                _ => 0
            };
        }

        public static bool PermiteDescargas(string plan)
        {
            return plan.ToLower() switch
            {
                "personal" or "familiar" or "empresarial" => true,
                _ => false
            };
        }
    }
}
