import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { generateStudyPlan } from "../api/studyPlanApi";
import PageContainer from "../components/PageContainer";
import ErrorMessage from "../components/ErrorMessage";
import PrimaryButton from "../components/PrimaryButton";

const DashboardPage = () => {
  const navigate = useNavigate();

  const [input, setInput] = useState("");
  const [generatedPlan, setGeneratedPlan] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const examples = [
    "Prepare me for a junior .NET backend interview in 14 days",
    "Create a 7-day React basics study plan",
    "Help me practice SQL queries for a technical interview",
  ];

  const handleGenerate = async (event) => {
    event.preventDefault();

    setError("");
    setGeneratedPlan(null);

    if (!input.trim()) {
      setError("Please enter what you want to study.");
      return;
    }

    if (input.trim().length < 10) {
      setError("Please describe your study goal with at least 10 characters.");
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
    <PageContainer>
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
          onChange={(event) => {
            setInput(event.target.value);
            setGeneratedPlan(null);
          }}
          rows="5"
          placeholder="Example: Prepare me for a junior .NET backend interview in 14 days"
          className="w-full rounded-lg border px-4 py-3 outline-none focus:border-blue-500"
        />

        <div className="mt-3 flex flex-wrap gap-2">
          {examples.map((example) => (
            <button
              key={example}
              type="button"
              onClick={() => setInput(example)}
              className="rounded-full border px-3 py-1 text-sm text-gray-600 hover:bg-gray-50"
            >
              {example}
            </button>
          ))}
        </div>

        <ErrorMessage message={error} />

        <PrimaryButton type="submit" disabled={loading} className="mt-4">
          {loading ? "Generating..." : "Generate Plan"}
        </PrimaryButton>
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
    </PageContainer>
  );
};

export default DashboardPage;