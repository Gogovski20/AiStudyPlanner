import { Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const LandingPage = () => {
  const { isAuthenticated } = useAuth();

  return (
    <main className="bg-gray-50">
      <section className="mx-auto grid min-h-screen max-w-6xl items-center gap-12 px-4 py-16 md:grid-cols-2">
        <div>
          <p className="mb-3 text-sm font-semibold uppercase tracking-wide text-blue-600">
            AI-powered learning assistant
          </p>

          <h1 className="text-4xl font-bold tracking-tight text-gray-900 md:text-5xl">
            Generate study plans and track your progress.
          </h1>

          <p className="mt-6 text-lg text-gray-600">
            AiStudyPlanner helps users create personalized study and interview
            preparation plans, save their history, manage tasks, and monitor
            learning progress.
          </p>

          <div className="mt-8 flex flex-wrap gap-4">
            <Link
              to={isAuthenticated ? "/dashboard" : "/register"}
              className="rounded-lg bg-blue-600 px-6 py-3 font-medium text-white hover:bg-blue-700"
            >
              {isAuthenticated ? "Go to Dashboard" : "Get Started"}
            </Link>

            <Link
              to={isAuthenticated ? "/history" : "/login"}
              className="rounded-lg border bg-white px-6 py-3 font-medium text-gray-700 hover:bg-gray-50"
            >
              {isAuthenticated ? "View History" : "Login"}
            </Link>
          </div>
        </div>

        <div className="rounded-2xl bg-white p-6 shadow-lg">
          <div className="mb-4 rounded-xl bg-blue-50 p-4">
            <p className="text-sm font-semibold text-blue-700">
              Example prompt
            </p>
            <p className="mt-1 text-gray-800">
              Prepare me for a junior .NET backend interview in 14 days.
            </p>
          </div>

          <div className="space-y-3">
            <div className="rounded-lg border p-4">
              Review ASP.NET Core fundamentals
            </div>
            <div className="rounded-lg border p-4">
              Practice Entity Framework Core questions
            </div>
            <div className="rounded-lg border p-4">
              Build a small REST API exercise
            </div>
          </div>

          <div className="mt-6 rounded-lg bg-gray-100 p-3 text-sm text-gray-600">
            Progress tracking, task editing, and study history included.
          </div>
        </div>
      </section>
    </main>
  );
};

export default LandingPage;