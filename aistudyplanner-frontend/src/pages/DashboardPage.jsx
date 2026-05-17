import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { generateStudyPlan } from "../api/studyPlanApi";

const DashboardPage = () => {
  const navigate = useNavigate();

  const [input, setInput] = useState("");
  const [generatedPlan, setGeneratedPlan] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleGenerate = async (event) => {
    event.preventDefault();

    setError("");
    setGeneratedPlan(null);

    if (!input.trim()) {
      setError("Please enter what you want to study.");
      return;
    }

    setLoading(true);

    try {
      const data = await generateStudyPlan(input);
      setGeneratedPlan(data);
    } catch (err) {
      setError(
        err.response?.data?.message ||
          err.response?.data?.Message ||
          "Failed to generate study plan."
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="mx-auto max-w-5xl px-4 py-10">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">
          Generate Study Plan
        </h1>
        <p className="mt-2 text-gray-600">
          Enter a study topic, programming skill, interview goal, or exam preparation goal.
        </p>
      </div>

      <form onSubmit={handleGenerate} className="rounded-2xl bg-white p-6 shadow">
        <label className="mb-2 block font-medium text-gray-700">
          What do you want to study?
        </label>

        <textarea
          value={input}
          onChange={(event) => setInput(event.target.value)}
          rows="5"
          placeholder="Example: Prepare me for a junior .NET backend interview in 14 days"
          className="w-full rounded-lg border px-4 py-3 outline-none focus:border-blue-500"
        />

        {error && (
          <div className="mt-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">
            {error}
          </div>
        )}

        <button
          disabled={loading}
          className="mt-4 rounded-lg bg-blue-600 px-6 py-3 font-medium text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {loading ? "Generating..." : "Generate Plan"}
        </button>
      </form>

      {generatedPlan && (
        <section className="mt-8 rounded-2xl bg-white p-6 shadow">
          <div className="mb-5 flex flex-col justify-between gap-4 md:flex-row md:items-start">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">
                Generated Plan
              </h2>

              <p className="mt-2 text-gray-600">
                Estimated time: {generatedPlan.estimatedTime}
              </p>

              <p className="text-gray-600">
                Priority: {generatedPlan.priority}
              </p>

              {generatedPlan.progress && (
                <p className="text-gray-600">
                  Progress: {generatedPlan.progress.percentage}%{" "}
                  ({generatedPlan.progress.completedTasks}/{generatedPlan.progress.totalTasks} tasks)
                </p>
              )}
            </div>

            {generatedPlan.id && (
              <button
                onClick={() => navigate(`/history/${generatedPlan.id}`)}
                className="rounded-lg border px-4 py-2 text-gray-700 hover:bg-gray-50"
              >
                Open Details
              </button>
            )}
          </div>

          <div className="space-y-3">
            {generatedPlan.tasks?.map((task) => (
              <div key={task.id} className="rounded-lg border p-4">
                <p className="text-gray-800">{task.title}</p>
              </div>
            ))}
          </div>
        </section>
      )}
    </main>
  );
};

export default DashboardPage;