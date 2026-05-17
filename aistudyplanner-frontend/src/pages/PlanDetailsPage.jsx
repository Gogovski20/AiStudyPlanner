import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  addTask,
  completeTask,
  deleteHistory,
  deleteTask,
  getHistoryById,
  incompleteTask,
  updateTaskTitle,
} from "../api/studyPlanApi";

const PlanDetailsPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  const [plan, setPlan] = useState(null);
  const [newTaskTitle, setNewTaskTitle] = useState("");
  const [editingTaskId, setEditingTaskId] = useState(null);
  const [editingTitle, setEditingTitle] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  const loadPlan = async () => {
    try {
      const data = await getHistoryById(id);
      setPlan(data);
    } catch (err) {
      setError(
        err.response?.data?.message ||
          err.response?.data?.Message ||
          "Failed to load study plan."
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPlan();
  }, [id]);

  const handleToggleTask = async (task) => {
    setError("");

    try {
      if (task.isCompleted) {
        await incompleteTask(id, task.id);
      } else {
        await completeTask(id, task.id);
      }

      await loadPlan();
    } catch (err) {
      setError(
        err.response?.data?.message ||
          err.response?.data?.Message ||
          "Failed to update task."
      );
    }
  };

  const handleDeleteTask = async (taskId) => {
    setError("");

    try {
      await deleteTask(id, taskId);
      await loadPlan();
    } catch (err) {
      setError(
        err.response?.data?.message ||
          err.response?.data?.Message ||
          "Failed to delete task."
      );
    }
  };

  const handleAddTask = async (event) => {
    event.preventDefault();
    setError("");

    if (!newTaskTitle.trim()) {
      setError("Task title cannot be empty.");
      return;
    }

    try {
      await addTask(id, newTaskTitle);
      setNewTaskTitle("");
      await loadPlan();
    } catch (err) {
      setError(
        err.response?.data?.message ||
          err.response?.data?.Message ||
          "Failed to add task."
      );
    }
  };

  const startEditing = (task) => {
    setEditingTaskId(task.id);
    setEditingTitle(task.title);
  };

  const cancelEditing = () => {
    setEditingTaskId(null);
    setEditingTitle("");
  };

  const handleSaveTitle = async (taskId) => {
    setError("");

    if (!editingTitle.trim()) {
      setError("Task title cannot be empty.");
      return;
    }

    try {
      await updateTaskTitle(id, taskId, editingTitle);
      setEditingTaskId(null);
      setEditingTitle("");
      await loadPlan();
    } catch (err) {
      setError(
        err.response?.data?.message ||
          err.response?.data?.Message ||
          "Failed to update task title."
      );
    }
  };

  const handleDeletePlan = async () => {
    const confirmed = window.confirm(
      "Are you sure you want to delete this study plan?"
    );

    if (!confirmed) return;

    setError("");

    try {
      await deleteHistory(id);
      navigate("/history");
    } catch (err) {
      setError(
        err.response?.data?.message ||
          err.response?.data?.Message ||
          "Failed to delete study plan."
      );
    }
  };

  if (loading) {
    return (
      <main className="mx-auto max-w-5xl px-4 py-10">
        <p className="text-gray-600">Loading study plan...</p>
      </main>
    );
  }

  if (!plan) {
    return (
      <main className="mx-auto max-w-5xl px-4 py-10">
        <p className="text-gray-600">Study plan not found.</p>
      </main>
    );
  }

  const progress = plan.progress?.percentage ?? 0;
  const completedTasks = plan.progress?.completedTasks ?? 0;
  const totalTasks = plan.progress?.totalTasks ?? 0;

  return (
    <main className="mx-auto max-w-5xl px-4 py-10">
      <div className="mb-8 flex flex-col justify-between gap-4 md:flex-row md:items-start">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            Study Plan Details
          </h1>

          <p className="mt-2 text-gray-600">
            {plan.input}
          </p>

          <div className="mt-4 flex flex-wrap gap-4 text-sm text-gray-600">
            <span>Estimated time: {plan.estimatedTime}</span>
            <span>Priority: {plan.priority}</span>
            <span>
              Progress: {progress}% ({completedTasks}/{totalTasks} tasks)
            </span>
          </div>
        </div>

        <button
          onClick={handleDeletePlan}
          className="rounded-lg bg-red-600 px-4 py-2 font-medium text-white hover:bg-red-700"
        >
          Delete Plan
        </button>
      </div>

      <div className="mb-6 h-3 overflow-hidden rounded-full bg-gray-200">
        <div
          className="h-3 rounded-full bg-blue-600"
          style={{ width: `${progress}%` }}
        />
      </div>

      {error && (
        <div className="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">
          {error}
        </div>
      )}

      <section className="rounded-2xl bg-white p-6 shadow">
        <h2 className="mb-4 text-xl font-bold text-gray-900">
          Tasks
        </h2>

        <div className="space-y-3">
          {plan.tasks?.map((task) => (
            <div
              key={task.id}
              className="flex flex-col gap-3 rounded-lg border p-4 md:flex-row md:items-center md:justify-between"
            >
              <div className="flex flex-1 items-center gap-3">
                <input
                  type="checkbox"
                  checked={task.isCompleted}
                  onChange={() => handleToggleTask(task)}
                  className="h-5 w-5"
                />

                {editingTaskId === task.id ? (
                  <input
                    value={editingTitle}
                    onChange={(event) => setEditingTitle(event.target.value)}
                    className="w-full rounded-lg border px-3 py-2 outline-none focus:border-blue-500"
                  />
                ) : (
                  <span
                    className={
                      task.isCompleted
                        ? "text-gray-400 line-through"
                        : "text-gray-800"
                    }
                  >
                    {task.title}
                  </span>
                )}
              </div>

              <div className="flex gap-2">
                {editingTaskId === task.id ? (
                  <>
                    <button
                      onClick={() => handleSaveTitle(task.id)}
                      className="rounded-lg bg-blue-600 px-3 py-2 text-sm text-white hover:bg-blue-700"
                    >
                      Save
                    </button>

                    <button
                      onClick={cancelEditing}
                      className="rounded-lg border px-3 py-2 text-sm hover:bg-gray-50"
                    >
                      Cancel
                    </button>
                  </>
                ) : (
                  <button
                    onClick={() => startEditing(task)}
                    className="rounded-lg border px-3 py-2 text-sm hover:bg-gray-50"
                  >
                    Edit
                  </button>
                )}

                <button
                  onClick={() => handleDeleteTask(task.id)}
                  className="rounded-lg border px-3 py-2 text-sm text-red-600 hover:bg-red-50"
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>

        <form onSubmit={handleAddTask} className="mt-6 flex flex-col gap-3 md:flex-row">
          <input
            value={newTaskTitle}
            onChange={(event) => setNewTaskTitle(event.target.value)}
            placeholder="Add a custom task"
            className="flex-1 rounded-lg border px-4 py-3 outline-none focus:border-blue-500"
          />

          <button className="rounded-lg bg-blue-600 px-5 py-3 font-medium text-white hover:bg-blue-700">
            Add Task
          </button>
        </form>
      </section>
    </main>
  );
};

export default PlanDetailsPage;