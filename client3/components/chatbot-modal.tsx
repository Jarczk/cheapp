'use client'
import { useState, useEffect, useRef } from 'react'
import { MessageCircle, Send } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog'
import { ScrollArea } from '@/components/ui/scroll-area'
import { useSendChatMessage, useChatHistory } from '@/lib/hooks'
import { ChatMessage } from '@/types/api'
import { motion, AnimatePresence } from 'framer-motion'

const SYSTEM_PROMPT =
  "Jesteś pomocnym asystentem sklepowym. Masz pomóc w wyborze produktu. Staraj się odpowiadać krótko i konkretnie. Nie rozpisuj się tylko szybko próbuj znaleźć produkt którego użytkownik szuka. Behave as a friendly shopping assistant who knows Cheapp's offers."

export function ChatbotModal() {
  const [open, setOpen] = useState(false)
  const [input, setInput] = useState('')
  const [sessionId, setSessionId] = useState<string>()
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const scrollRef = useRef<HTMLDivElement>(null)

  /* 1 —— history sync on modal open */
  const { data: history } = useChatHistory(open ? sessionId : undefined)
  useEffect(() => {
    if (!history) return
    setMessages(history)
  }, [history])

  /* 2 —— auto-scroll */
  useEffect(() => {
    scrollRef.current?.scrollTo(0, scrollRef.current.scrollHeight)
  }, [messages])

  /* 3 —— send hook */
  const { mutateAsync: send, isPending } = useSendChatMessage()

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!input.trim() || isPending) return

    const userMsg: ChatMessage = {
      id: crypto.randomUUID(),
      role: 'user',
      content: input.trim(),
      timestamp: new Date().toISOString(),
    }
    setMessages((m) => [...m, userMsg])
    setInput('')

    try {
      const res = await send({
        message: userMsg.content,
        sessionId,
        systemPrompt: sessionId ? undefined : SYSTEM_PROMPT,
      })

      setSessionId(res.sessionId)                       // ← keep id
      const botMsg: ChatMessage = {
        id: crypto.randomUUID(),
        role: 'assistant',
        content: res.answer,
        timestamp: new Date().toISOString(),
      }
      setMessages((m) => [...m, botMsg])
    } catch {
      setMessages((m) => [
        ...m,
        { id: crypto.randomUUID(), role: 'assistant', content: '⚠️ Error, try again.', timestamp: new Date().toISOString() },
      ])
    }
  }

  /* 4 —— optional: reset on modal close */
  const handleOpenChange = (o: boolean) => {
    if (!o) {
      setSessionId(undefined)
      setMessages([])
    }
    setOpen(o)
  }

  return (
    <motion.div initial={{ scale: 0 }} animate={{ scale: 1 }} className="fixed bottom-6 right-6 z-50">
      <Dialog open={open} onOpenChange={handleOpenChange}>
        <DialogTrigger asChild>
          <Button size="lg" className="rounded-full h-14 w-14 shadow">
            chat
            <MessageCircle className="h-6 w-6" />
          </Button>
        </DialogTrigger>

        <DialogContent className="sm:max-w-md h-[600px] flex flex-col">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <MessageCircle className="h-5 w-5" /> Shopping Assistant
            </DialogTitle>
          </DialogHeader>

          {/* messages */}
          <ScrollArea className="flex-1 pr-4" ref={scrollRef}>
            <AnimatePresence>
              {messages.map((m) => (
                <motion.div
                  key={m.id}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0 }}
                  className={`flex ${m.role === 'user' ? 'justify-end' : 'justify-start'}`}
                >
                  <div
                    className={`max-w-[80%] rounded-lg px-3 py-2 whitespace-pre-wrap ${
                      m.role === 'user' ? 'bg-primary text-primary-foreground' : 'bg-muted text-muted-foreground'
                    }`}
                    dangerouslySetInnerHTML={{ __html: m.content }}
                  ></div>
                </motion.div>
              ))}
              {isPending && (
                <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="flex justify-start">
                  <div className="bg-muted rounded-lg px-3 py-2">…</div>
                </motion.div>
              )}
            </AnimatePresence>
          </ScrollArea>

          {/* input */}
          <form onSubmit={onSubmit} className="flex gap-2 pt-4 border-t">
            <Input
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Ask me about products…"
              disabled={isPending}
              className="flex-1"
            />
            <Button type="submit" size="icon" disabled={!input.trim() || isPending}>
              <Send className="h-4 w-4" />
            </Button>
          </form>
        </DialogContent>
      </Dialog>
    </motion.div>
  )
}
