import React, { useState, useEffect } from 'react';
import { Form, Button, Table } from 'react-bootstrap';
import axios from 'axios';
import { saveAs } from 'file-saver';
import { Packer, Paragraph, HeadingLevel, TableCell, TableRow } from 'docx';

const TaskForm = () => {
  const [tasks, setTasks] = useState([]);
  const [description, setDescription] = useState('');

  const userId = localStorage.getItem('userId');

  useEffect(() => {
    const fetchTasks = async () => {
      try {
        const response = await axios.get(`https://localhost:7010/api/Task/tasks/${userId}`);
        setTasks(response.data);
      } catch (error) {
        console.log(error.response.data);
      }
    };

    fetchTasks();
  }, [userId]);

  const handleAddTask = async (e) => {
    e.preventDefault();
    try {
      await axios.post(`https://localhost:7010/api/Task/users/${userId}/tasks`, { description: description });
      setDescription('');
      // Refresh the task list
      const response = await axios.get(`https://localhost:7010/api/Task/tasks/${userId}`);
      setTasks(response.data);
    } catch (error) {
      console.log(error.response.data);
    }
  };

  const handleCompleteTask = async (taskId) => {
    try {
      await axios.put(`https://localhost:7010/api/Task/${userId}/tasks/${taskId}/complete`);
      // Refresh the task list
      const response = await axios.get(`https://localhost:7010/api/Task/tasks/${userId}`);
      setTasks(response.data);
    } catch (error) {
      console.log(error.response.data);
    }
  };

  const handleResetTasks = async () => {
    try {
      const response = await axios.post(`https://localhost:7010/api/Task/ResetTasks?userId=${userId}`);
      if (response && response.data) {
        const completedTasks = response.data;
  
        if (completedTasks.length > 0) {
          const doc = new Document();
  
          // Add header for completed tasks
          doc.addSection({
            children: [
              new Paragraph({
                text: 'Completed Tasks',
                heading: HeadingLevel.HEADING_1,
              }),
              new Paragraph(),
            ],
          });
  
          // Add completed tasks to a table
          const table = doc.addTable({
            rows: [
              new TableRow({
                children: [
                  new TableCell({ children: [new Paragraph('Description')] }),
                  new TableCell({ children: [new Paragraph('Completed')] }),
                ],
              }),
              ...completedTasks.map((task) => {
                return new TableRow({
                  children: [
                    new TableCell({ children: [new Paragraph(task.description)] }),
                    new TableCell({ children: [new Paragraph('Yes')] }),
                  ],
                });
              }),
            ],
          });
  
          // Save the document as a .docx file
          const buffer = await Packer.toBuffer(doc);
          saveAs(new Blob([buffer]), 'completed_tasks.docx');
        }
      }
    } catch (error) {
      console.log(error);
    }
  };
  
  
  

  return (
    <>
      <div className="d-flex justify-content-center">
        <div className="text-center">
          <h2>Tasks</h2>
        </div>
        <Form onSubmit={handleAddTask}>
          <Form.Group controlId="formBasicDescription">
            <Form.Label>Description</Form.Label>
            <Form.Control type="text" placeholder="Enter task description" value={description} onChange={(e) => setDescription(e.target.value)} />
          </Form.Group>

          <Button variant="primary" type="submit">
            Add Task
          </Button>
          <Button variant="danger" onClick={handleResetTasks}>
            Reset Completed Tasks
           </Button>

        </Form>
      </div>
      <Table striped bordered hover>
        <thead>
          <tr>
            <th>Description</th>
            <th>Completed</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          {tasks.map((task) => (
            <tr key={task.id}>
              <td>{task.description}</td>
              <td>{task.completed ? 'Yes' : 'No'}</td>
              <td>
                {!task.completed && (
                  <Button variant="success" onClick={() => handleCompleteTask(task.id)}>
                    Mark as Completed
                  </Button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </Table>
    </>
  );
};

export default TaskForm;
