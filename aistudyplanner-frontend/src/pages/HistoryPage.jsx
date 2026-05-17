import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getHistory } from "../api/studyPlanApi";

const HistoryPage = () => {
  const [history, setHistory] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadHistory = async () => {
      try {
        const data = await getHistory();
        setHistory(data);
      } catch (err) {
        setError(
          err.response?.data?.message ||
            err.response?.data?.Message ||
            "Failed to load study history."
        );
      } finally {
        setLoading(false);
      }
    };

    loadHistory();
  }, []);

  if (loading) {
    return (
      <main className="mx-auto max-w-5xl px-4 py-10">
        <p className="text-gray-600">Loading history...</p>
      </main>
    );
  }

  return (
    <main className="mx-auto max-w-5xl px-4 py-10">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">
          Study History
        </h1>
        <p className="mt-2 text-gray-600">
          View your previously generated study plans.
        </p>
      </div>

      {error && (
        <div className="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {history.length === 0 ? (
        <div className="rounded-2xl bg-white p-8 text-center shadow">
          <p className="text-gray-600">No study plans yet.</p>

          <Link
            to="/dashboard"
            className="mt-4 inline-block rounded-lg bg-blue-600 px-5 py-2 font-medium text-white hover:bg-blue-700"
          >
            Generate your first plan
          </Link>
        </div>
      ) : (
        <div className="grid gap-4">
          {history.map((item) => (
            <Link
              key={item.id}
              to={`/history/${item.id}`}
              className="rounded-2xl bg-white p-5 shadow hover:shadow-md"
            >
              <h2 className="font-semibold text-gray-900">
                {item.input}
              </h2>

              <div className="mt-3 flex flex-wrap gap-4 text-sm text-gray-600">
                <span>Estimated time: {item.estimatedTime}</span>
                <span>Priority: {item.priority}</span>
                {item.progress && (
                  <span>
                    Progress: {item.progress.percentage}% ({item.progress.completedTasks}/{item.progress.totalTasks} tasks)
                  </span>
                )}
              </div>
            </Link>
          ))}
        </div>
      )}
    </main>
  );
};

export default HistoryPage;